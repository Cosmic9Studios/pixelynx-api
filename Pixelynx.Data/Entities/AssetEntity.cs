using System;

namespace Pixelynx.Data.Entities
{
    public class AssetEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string StorageBucket { get; set; }
        public Guid StorageId { get; set; }
    }
}