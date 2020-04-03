using System;
using System.ComponentModel.DataAnnotations.Schema;
using Pixelynx.Data.Enums;

namespace Pixelynx.Data.Entities
{
    public class TransactionEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public TransactionType Type { get; set; }
        public int Value { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; }
    }
}