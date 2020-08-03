using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pixelynx.Logic.Model
{
    public class AssetMetadata 
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public List<string> Tags { get; set; }
        public string Thumbnail { get; set; }
        public int Background { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Core.AssetType Type { get; set; }
    }
}