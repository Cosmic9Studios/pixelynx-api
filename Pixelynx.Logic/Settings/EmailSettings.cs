namespace Pixelynx.Logic.Settings
{
    public class EmailSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Sender { get; set; }

        public string RegistrationTemplate { get; set; }
        public string ForgotPasswordTemplate { get; set; }
    }
}