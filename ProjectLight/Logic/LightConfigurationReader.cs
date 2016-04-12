using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ProjectLight.Models;

namespace ProjectLight.Logic
{
    public class LightConfigurationReader
    {
        public async Task<ImageMap> CreateFromFileAsync(string fileLocation)
        {
            using (XmlReader reader = XmlReader.Create(File.OpenText(fileLocation), new XmlReaderSettings {Async = true}))
            {
                var stack = new ConcurrentStack<ImageRectangle>();
                while (await reader.ReadAsync())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (reader.Name)
                            {
                                case "Rectangle":
                                    var x = Convert.ToUInt32(reader.GetAttribute("x"));
                                    var y = Convert.ToUInt32(reader.GetAttribute("y"));
                                    var width = Convert.ToUInt32(reader.GetAttribute("width"));
                                    var height = Convert.ToUInt32(reader.GetAttribute("height"));
                                    var id = reader.GetAttribute("id");
                                    stack.Push(new ImageRectangle
                                    {
                                        X = x,
                                        Y = y,
                                        Width = width,
                                        Height = height,
                                        Id = id
                                    });
                                    break;
                                case "Light":
                                    ImageRectangle rect;
                                    stack.TryPeek(out rect);
                                    rect.AffectedLights = rect.AffectedLights ?? new List<Light>();
                                    rect.AffectedLights.Add(new Light
                                    {
                                        Id = reader.GetAttribute("id")
                                    });
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Comment:
                        case XmlNodeType.EndElement:
                            string name = reader.Name;
                            break;
                    }
                }
                return new ImageMap {Blocks = stack.ToList()};
            }
        }
    }
}