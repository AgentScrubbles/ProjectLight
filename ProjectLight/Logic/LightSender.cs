using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using ProjectLight.Interfaces;

namespace ProjectLight.Logic
{
    public class LightSender : ISendableColor
    {
        public async Task SendColorAsync(string key, Color color)
        {
            Debug.WriteLine("Light {0} has been sent color {1}", key, color);
        }
    }
}
