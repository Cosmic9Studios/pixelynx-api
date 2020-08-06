using Newtonsoft.Json;

namespace Pixelynx.Logic.Model.Email
{
    public class RegistrationData
    {
        [JsonProperty("Receipient")]
        public string Receipient { get; set; }

        [JsonProperty("ButtonUrl")]
        public string ButtonUrl { get; set; }
    }
}