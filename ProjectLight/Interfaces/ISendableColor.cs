using System.Threading.Tasks;
using Windows.UI;

namespace ProjectLight.Interfaces
{
    public interface ISendableColor
    {
        Task SendColorAsync(string key, Color color);
    }
}
