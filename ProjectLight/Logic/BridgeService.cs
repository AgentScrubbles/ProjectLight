using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Q42.HueApi;
using Q42.HueApi.Interfaces;
using Q42.HueApi.WinRT;

namespace ProjectLight.Logic
{
    public class BridgeService
    {
        public class HueConfiguration
        {
            public string ProjectName { get; set; }
            public string Key { get; set; }
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
                file = await ApplicationData.Current.LocalFolder.GetFileAsync("HueConfig");
            }
            catch(FileNotFoundException fe)
            {
                var xorgs = new HueConfiguration
                {
                    Key = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 18),
                    ProjectName = "ProjectLight"
                };
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("HueConfig",
                    CreationCollisionOption.OpenIfExists);
                try
                {
                    await new LocalHueClient(SelectedBridge).RegisterAsync(xorgs.ProjectName, xorgs.Key);
                }
                catch (Exception e)
                {
                    return null;
                }
                await FileIO.WriteTextAsync(file,
                    JsonConvert.SerializeObject(xorgs));
            }
            var read = await FileIO.ReadTextAsync(file);
            var config = JsonConvert.DeserializeObject<HueConfiguration>(read);
            return config;
        }

        public async Task<ICollection<string>> GetLights()
        {
            var config = await GetConfiguration();
            ILocalHueClient client = new LocalHueClient(SelectedBridge);
             client.Initialize(config.Key);
            var lights = await client.GetLightsAsync();
            return lights.Select(k => k.Id).ToList();
        }
    }
}
