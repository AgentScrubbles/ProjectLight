using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI;

namespace ProjectLight.Logic
{
    public class DeviceCapturer
    {
        public MediaCapture MediaCapture { get; private set; }
        public DeviceInformation SelectedDevice { get; private set; }
        public BitmapTransformer Transformer { get; set; }
        public bool IsPreviewing { get; set; }
        public bool IsCapturing { get; set; }
        private bool _isInitialized { get; set; }

        public DeviceCapturer()
        {
            _isInitialized = false;
        }

        public async Task<IDictionary<string, string>> GetDevicesAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices.ToDictionary(k => k.Name, v => v.Id);
        }

        public async Task SetSelectedDevice(string deviceId)
        {
            SelectedDevice = await DeviceInformation.CreateFromIdAsync(deviceId);
        }

        public async Task InitializeCapture()
        {
            MediaCapture = new MediaCapture();

            // Register for a notification when video recording has reached the maximum time and when something goes wrong

            var mediaInitSettings = new MediaCaptureInitializationSettings { VideoDeviceId = SelectedDevice.Id };


            await MediaCapture.InitializeAsync(mediaInitSettings);
            _isInitialized = true;
        }

        public async Task StartCapture()
        {
            if (!_isInitialized) { await InitializeCapture(); }
            IsCapturing = true;
            while (IsCapturing)
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await MediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    await Transformer.Process(decoder);

                }
            }
        }
    }
}
