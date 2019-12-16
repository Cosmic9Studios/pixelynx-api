using System;

namespace Pixelynx.Core
{
    public class Asset
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
        public string ThumbnailUri { get; set; }
        public string PreviewUri { get; set; }
    }
}
