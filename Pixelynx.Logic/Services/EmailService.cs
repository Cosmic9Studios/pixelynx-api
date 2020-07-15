using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Settings;

namespace Pixelynx.Logic.Services
{
    public class EmailService : IEmailService
    {
        private EmailSettings emailSettings;
        private IVaultService vaultService;

        public EmailService(IOptions<EmailSettings> emailSettings, IVaultService vaultService)
        {
            this.emailSettings = emailSettings?.Value;
            this.vaultService = vaultService;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
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
                    client.Authenticate(emailSettings.Username, (await vaultService.GetAuthSecrets()).SendgridApiKey);
                }

				client.Send(message);
                client.Disconnect(true);
			}
        }

        public async Task SendEmailFromTemplateAsync(string to, string subject, string templateName, Dictionary<string, string> variables)
        {
            var fileText = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/EmailTemplates/{templateName}.html");
            foreach (var variable in variables)
            {
                fileText = fileText.Replace($"[{variable.Key}]", variable.Value);
            }
            await SendEmailAsync(to, subject, fileText);
        }
    }
}