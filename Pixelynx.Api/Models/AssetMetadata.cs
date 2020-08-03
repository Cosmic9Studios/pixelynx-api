using System;

namespace Pixelynx.Api.Models
{
    public class AssetMetadata
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public int Cost { get; set; }
    }
}