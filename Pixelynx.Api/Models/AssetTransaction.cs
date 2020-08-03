using System;

namespace Pixelynx.Api.Models
{
    public class AssetTransaction
    {
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string TransactionId { get; set; }
        public int Cost { get; set; }
        public int TransactionTotal { get; set; }
    }
}