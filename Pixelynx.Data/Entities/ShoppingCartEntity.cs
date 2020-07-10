using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public enum CartStatus
    {
        New,
        Complete,
        Abandoned
    }
    
    public class CartEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<CartItemEntity> CartItems { get; set; }
        public CartStatus Status { get; set; }
        
        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
    }
}