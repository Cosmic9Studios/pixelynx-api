using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pixelynx.Data.Entities
{
    public class PaymentDetailsEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CustomerId { get; set; }
        public string DefaultPaymentMethodId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
    }
}