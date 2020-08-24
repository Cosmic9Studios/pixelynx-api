using System.Collections.Generic;
using Pixelynx.Logic.Model;

namespace Pixelynx.Logic.Models
{
    public class CheckoutData
    {
        public float Total { get; set; }
        public List<AssetMetadata> AssetMetadata { get; set; }
        public bool Succeeded { get; set; }
        public string Error { get; set; }
    }
}