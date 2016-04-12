using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ProjectLight.Interfaces;
using ProjectLight.Models;

namespace ProjectLight.Logic
{
    public class BitmapTransformer
    {
        public ImageMap Map { get; private set; }
        public ISendableColor Sendable { get; private set; }

        public IDictionary<string, BitmapBounds> Bounds
        {
            get
            {
                return Map.Blocks.ToDictionary(k => k.Key, k => new BitmapBounds
                {
                    Height = k.Value.Height,
                    Width = k.Value.Width,
                    X = k.Value.X,
                    Y = k.Value.Y
                });
            }
        }

        public BitmapTransformer(ImageMap map, ISendableColor sendable)
        {
            Map = map;
            Sendable = sendable;
        }

        public async Task Process(BitmapDecoder decoder)
        {
            var bitmaps = await Split(decoder);
            bitmaps.AsParallel().ForAll(async k =>  
            {
                foreach (var light in Map.Blocks[k.Key].AffectedLights)
                {
                    var color = await GetColorFromBitmap(k.Value);
                    await Sendable.SendColorAsync(light.Id, color);
                }
            });
        }
        
        public async Task<IDictionary<string, BitmapDecoder>> Split(BitmapDecoder decoder)
        {
            var ras = new InMemoryRandomAccessStream();
            var enc = await BitmapEncoder.CreateForTranscodingAsync(ras, decoder);
            enc.BitmapTransform.ScaledHeight = 800;
            enc.BitmapTransform.ScaledWidth = 600;
            var dict = new ConcurrentDictionary<string, BitmapDecoder>();
            foreach(var kvPair in Bounds)
            {
                enc.BitmapTransform.Bounds = kvPair.Value;

                // write out to the stream
                try
                {
                    await enc.FlushAsync();
                }
                catch (Exception ex)
                {
                    var s = ex.ToString();
                }

                BitmapDecoder dec = await BitmapDecoder.CreateAsync(ras);
                
                dict[kvPair.Key] = dec;
            }
            return dict;
        }

        public async Task<Color> GetColorFromBitmap(BitmapDecoder decoder)
        {

            //Create a transform to get a 1x1 image
            var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

            //Get the pixel provider
            var pixels = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Ignore,
                myTransform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            //Get the bytes of the 1x1 scaled image
            var bytes = pixels.DetachPixelData();

            //read the color 
            return Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
        }
    }
}
