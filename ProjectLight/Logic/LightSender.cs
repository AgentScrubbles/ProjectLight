using System;
using System.Threading.Tasks;
using Windows.UI;
using ProjectLight.Interfaces;

namespace ProjectLight.Logic
{
    public class LightSender : ISendableColor
    {
        public async Task SendColorAsync(string key, Color color)
        {
            //throw new NotImplementedException();
        }
    }
}
