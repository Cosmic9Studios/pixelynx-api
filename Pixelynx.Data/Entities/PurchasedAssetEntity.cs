using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public class PurchasedAssetEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AssetId { get; set; }
        public string TransactionId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set ;}

        [ForeignKey("AssetId")]
        public AssetEntity Asset { get; set; }
    }
}