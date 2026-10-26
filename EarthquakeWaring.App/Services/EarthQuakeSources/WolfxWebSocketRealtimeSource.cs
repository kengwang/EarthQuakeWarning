using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EarthquakeWaring.App.Infrastructure.Models.EarthQuakeModels;
using EarthquakeWaring.App.Infrastructure.Models.SettingModels;
using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using Microsoft.Extensions.Logging;

namespace EarthquakeWaring.App.Services.EarthQuakeSources;

/// <summary>Receives all Wolfx EEW messages over a resilient WebSocket connection.</summary>
public sealed class WolfxWebSocketRealtimeSource : IEarthquakeRealtimeSource
{
    private static readonly Uri Endpoint = new("wss://ws-api.wolfx.jp/all_eew");
    private static readonly TimeSpan StaleTimeout = TimeSpan.FromSeconds(90);
    private const int MaximumMessageBytes = 1024 * 1024;
    private readonly WolfxMessageParser _parser;
    private readonly IWolfxReportCache _cache;
    private readonly WolfxHttpQuerySource _querySource;
    private readonly ILogger<WolfxWebSocketRealtimeSource> _logger;

    /// <summary>Initializes the Wolfx WebSocket source.</summary>
    public WolfxWebSocketRealtimeSource(
        WolfxMessageParser parser,
        IWolfxReportCache cache,
        WolfxHttpQuerySource querySource,
        ILogger<WolfxWebSocketRealtimeSource> logger)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _querySource = querySource ?? throw new ArgumentNullException(nameof(querySource));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SourceId => EarthquakeSourceIds.WolfxWebSocket;

    /// <inheritdoc />
    public async IAsyncEnumerable<EarthquakeReport> ListenAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<EarthquakeReport>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true
        });
        var producer = ProduceAsync(channel.Writer, cancellationToken);
        try
        {
            await foreach (var report in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                yield return report;
        }
        finally
        {
            try
            {
                await producer.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Normal source shutdown.
            }
        }
    }

    private async Task ProduceAsync(ChannelWriter<EarthquakeReport> writer, CancellationToken cancellationToken)
    {
        var retry = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var socket = new ClientWebSocket();
                try
                {
                    await socket.ConnectAsync(Endpoint, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Connected to Wolfx WebSocket at {Endpoint}", Endpoint);
                    retry = 0;

                    var snapshots = await _querySource.GetCurrentSnapshotsAsync(cancellationToken).ConfigureAwait(false);
                    foreach (var snapshot in snapshots)
                        await writer.WriteAsync(snapshot, cancellationToken).ConfigureAwait(false);

                    while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                    {
                        var json = await ReceiveMessageAsync(socket, cancellationToken).ConfigureAwait(false);
                        if (json is null)
                            break;

                        WolfxMessageParseResult result;
                        try
                        {
                            result = _parser.ParseWebSocket(json);
                        }
                        catch (Exception exception)
                        {
                            _logger.LogWarning(exception, "Ignoring malformed Wolfx WebSocket message");
                            continue;
                        }

                        if (result.ShouldReplyPing)
                            await SendPingAsync(socket, cancellationToken).ConfigureAwait(false);

                        if (result.Reports.Count == 0 && result.MessageType is not "heartbeat" and not "pong")
                            _logger.LogWarning("Ignoring unsupported Wolfx message type {MessageType}", result.MessageType);

                        foreach (var report in result.Reports)
                        {
                            _cache.Add(report);
                            if (!result.IsHistory)
                                await writer.WriteAsync(report, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Wolfx WebSocket connection ended unexpectedly");
                }
                finally
                {
                    await CloseQuietlyAsync(socket).ConfigureAwait(false);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    var delay = GetRetryDelay(retry++);
                    _logger.LogInformation("Reconnecting to Wolfx WebSocket in {Delay}", delay);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            writer.TryComplete();
        }
    }

    private static async Task<string?> ReceiveMessageAsync(
        ClientWebSocket socket,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        try
        {
            using var message = new MemoryStream();
            while (true)
            {
                using var staleSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                staleSource.CancelAfter(StaleTimeout);
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), staleSource.Token)
                    .ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                    return null;
                if (result.MessageType != WebSocketMessageType.Text)
                    continue;
                if (message.Length + result.Count > MaximumMessageBytes)
                    throw new InvalidDataException("Wolfx WebSocket message exceeded the one-megabyte limit.");
                await message.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
                if (result.EndOfMessage)
                    return Encoding.UTF8.GetString(message.GetBuffer(), 0, checked((int)message.Length));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static Task SendPingAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes("ping");
        return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
    }

    private static TimeSpan GetRetryDelay(int retry)
    {
        var delays = new[] { 1, 2, 4, 8, 15, 30 };
        var seconds = delays[Math.Min(retry, delays.Length - 1)];
        return TimeSpan.FromMilliseconds(seconds * 1000 + Random.Shared.Next(0, 500));
    }

    private static async Task CloseQuietlyAsync(ClientWebSocket socket)
    {
        if (socket.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
            return;
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Source stopped", timeout.Token)
                .ConfigureAwait(false);
        }
        catch
        {
            socket.Abort();
        }
    }
}
