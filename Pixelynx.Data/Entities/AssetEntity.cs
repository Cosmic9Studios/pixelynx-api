using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public class AssetEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid StorageId { get; set; }
        public string Name { get; set; }
        public string StorageBucket { get; set; }
        public string MediaStorageBucket { get; set; }
        public int AssetType { get; set; }
        public string FileHash { get; set; }
        public long Price { get; set; }

        [ForeignKey("ParentId")]
        public AssetEntity Parent { get; set; }
    }
}