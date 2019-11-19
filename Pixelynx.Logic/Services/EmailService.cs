using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Settings;

namespace Pixelynx.Logic.Services
{
    public class EmailService : IEmailService
    {
        private EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            this.emailSettings = emailSettings?.Value;
        }

        public void SendEmail(string to, string subject, string body)
        {
            var message = new MimeMessage();
            var bodyBuilder = new BodyBuilder();

			message.From.Add(new MailboxAddress(subject, emailSettings.Sender));
			message.To.Add(new MailboxAddress(to));
			message.Subject = subject;
            bodyBuilder.HtmlBody = body;

			message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient()) 
            {
				// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
				client.ServerCertificateValidationCallback = (s,c,h,e) => true;

				client.Connect(emailSettings.Server, emailSettings.Port, false);

				if (!string.IsNullOrWhiteSpace(emailSettings.Username))
                {
                    client.Authenticate(emailSettings.Username, emailSettings.Password);
                }

				client.Send(message);
                client.Disconnect(true);
			}
        }
    }
}