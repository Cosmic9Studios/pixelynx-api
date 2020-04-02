using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public class PaymentEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CustomerId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
    }
}