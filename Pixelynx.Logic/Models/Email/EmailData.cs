using Newtonsoft.Json;

namespace Pixelynx.Logic.Model.Email
{
    public class EmailData 
    {
        [JsonProperty("Receipient")]
        public string Receipient { get; set; } 
    }
}