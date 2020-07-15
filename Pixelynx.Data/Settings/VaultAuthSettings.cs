namespace Pixelynx.Data.Settings
{
    public class VaultAuthSettings
    {
        public string JWTSecret { get; set; }
        public string StripeSecretKey { get; set; }
        public string StripeEndpointSecret { get; set; }
        public string SendgridApiKey { get; set; }
    }
}