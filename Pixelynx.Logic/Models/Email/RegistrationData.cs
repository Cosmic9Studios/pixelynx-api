using Newtonsoft.Json;

namespace Pixelynx.Logic.Model.Email
{
    public class RegistrationData : EmailData
    {
        [JsonProperty("ButtonUrl")]
        public string ButtonUrl { get; set; }
    }
}