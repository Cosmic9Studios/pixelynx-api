using System.IO;

namespace Pixelynx.Logic.Model
{
    public class AssetData
    {
        public MemoryStream DataStream { get; set; }
        public MemoryStream ThumbnailStream { get; set; }
        public AssetMetadata Metadata { get; set; }
    }
}