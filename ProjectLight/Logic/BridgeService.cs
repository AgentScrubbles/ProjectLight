using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Newtonsoft.Json;
using ProjectLight.Exceptions;
using ProjectLight.Extensions;
using ProjectLight.Interfaces;
using Q42.HueApi;
using Q42.HueApi.Interfaces;

namespace ProjectLight.Logic
{
    public class BridgeService : ISendableColor
    {
        private const string ConfigFileName = "HueConfig";
        private ILocalHueClient _client;
        public ConcurrentDictionary<string, Light> AvailableLights { get; private set; } 

        public class HueConfiguration
        {
            public string ProjectName { get; set; }
            public string Key { get; set; }
            public string AppKey { get; set; }
        }

        public string SelectedBridge { get; set; }

        public async Task<ICollection<string>> GetBridges()
        {
            var locator = new HttpBridgeLocator();
            var bridges = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(5));
            return bridges.ToList();
        }

        private async Task<HueConfiguration> GetConfiguration()
        {
            StorageFile file;
            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(ConfigFileName);
            }
            catch(FileNotFoundException fe)
            {
                var xorgs = new HueConfiguration
                {
                    Key = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 18),
                    ProjectName = "ProjectLight"
                };
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(ConfigFileName,
                        CreationCollisionOption.OpenIfExists);
                try
                {
                    var client = new LocalHueClient(SelectedBridge);
                    var appKey = await client.RegisterAsync(xorgs.ProjectName, xorgs.Key);
                    client.Initialize(appKey);
                }
                catch (Exception e)
                {
                    await file.DeleteAsync();
                    throw new RequiresUserInteractionException("User needs to press the Hue connection button", e);
                }
                await FileIO.WriteTextAsync(file,
                    JsonConvert.SerializeObject(xorgs));
            }
            var read = await FileIO.ReadTextAsync(file);
            var config = JsonConvert.DeserializeObject<HueConfiguration>(read);
            return config;
        }

        public async Task DeleteConfiguration()
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(ConfigFileName);
            if (file != null)
            {
                await file.DeleteAsync();
            }
        }

        public async Task<IDictionary<string, string>> GetLights()
        {
            var config = await GetConfiguration();
            _client = new LocalHueClient(SelectedBridge);
            _client.Initialize(config.AppKey);
            var lights = (await _client.GetLightsAsync()).ToList();
            AvailableLights = lights.ToConcurrentDictionary(k => k.Id, v => v);
            return lights.ToConcurrentDictionary(k => k.Id, v => v.Name);
        }

        public async Task SendColorAsync(string key, Color color)
        {
            var lightCommand = new LightCommand {};
            lightCommand.SetColor(color.ToString().Substring(0, 7));
            await _client.SendCommandAsync(lightCommand, new [] {key});
            Debug.WriteLine("Light {0} has been sent color {1}", key, color);
        }
    }
}
