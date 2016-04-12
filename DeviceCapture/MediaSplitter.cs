using System.Threading.Tasks;         // Used to implement asynchronous methods
using Windows.Devices.Enumeration;    // Used to enumerate cameras on the device
using Windows.Devices.Sensors;        // Orientation sensor is used to rotate the camera preview
using Windows.Graphics.Display;       // Used to determine the display orientation
using Windows.Graphics.Imaging;       // Used for encoding captured images
using Windows.Media;                  // Provides SystemMediaTransportControls
using Windows.Media.Capture;          // MediaCapture APIs
using Windows.Media.MediaProperties;  // Used for photo and video encoding
using Windows.Storage;                // General file I/O
using Windows.Storage.FileProperties; // Used for image file encoding
using Windows.Storage.Streams;        // General file I/O
using Windows.System.Display;         // Used to keep the screen awake during preview and capture
using Windows.UI.Core;                // Used for updating UI from within async operations

namespace DeviceCapture
{
    public class MediaSplitter
    {
        public async Task<IDictionary<string, Guid>> GetAvailableDevices()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
        }
    }
}
