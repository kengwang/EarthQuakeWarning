using EarthquakeWaring.App.Infrastructure.ServiceAbstraction;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.Linq;

namespace EarthquakeWaring.App.Services
{
    public class VolumeManager : IVolumeManager
    {
        public void SetVolumeToMax()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                IList<MMDevice> outputDevices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
                if (outputDevices.Count() > 0)
                {
                    foreach (var device in outputDevices)
                    {
                        using (device)
                        {
                            device.AudioEndpointVolume.Mute = false;
                            device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                        }
                    }
                }
            }
        }
    }
}
