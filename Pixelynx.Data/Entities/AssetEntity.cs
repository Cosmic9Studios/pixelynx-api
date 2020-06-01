using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Pixelynx.Data.Entities
{
    public class AssetEntity
    {
        public Guid Id { get; set; }
        public Guid UploaderId { get; set; }
        public Guid? ParentId { get; set; }
        public Guid StorageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StorageBucket { get; set; }
        public string MediaStorageBucket { get; set; }
        public int AssetType { get; set; }
        public string FileHash { get; set; }
        public long Price { get; set; }
        public List<AssetEntity> Children { get; set; }

        [ForeignKey("ParentId")]
        public AssetEntity Parent { get; set; }

        [ForeignKey("UploaderId")]
        public UserEntity Uploader { get; set; }
    }
}