using HotChocolate.Types;

namespace AssetStore.Api.Types
{
    public class Asset
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
    }
}