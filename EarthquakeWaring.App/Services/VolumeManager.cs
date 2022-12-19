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
                using (var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia))
                {
                    device.AudioEndpointVolume.Mute = false;
                    device.AudioEndpointVolume.MasterVolumeLevelScalar = 1;
                }
            }
        }
    }
}
