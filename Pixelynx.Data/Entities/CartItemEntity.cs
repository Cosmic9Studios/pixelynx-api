using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public class CartItemEntity
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid AssetId { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("CartId")] 
        public CartEntity Cart { get; set; }
        
        [ForeignKey("AssetId")]
        public AssetEntity Asset { get; set; }
    }
}