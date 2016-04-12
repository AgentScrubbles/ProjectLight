using System.Collections.Generic;

namespace ProjectLight.Models
{
    public class ImageMap
    {
        public List<ImageRectangle> Blocks { get; set; }
    }

    public class ImageRectangle
    {
        public string Id { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public ICollection<Light> AffectedLights { get; set; }
    }
    public class Light
    {
        public string Id { get; set; }
    }
}
