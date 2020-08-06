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
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Pixelynx.Logic.Services
{
    public enum EmailTemplate 
    {
        Registration,
        ForgotPassword
    }

    public class EmailService : IEmailService
    {
        private EmailSettings emailSettings;
        private IVaultService vaultService;

        public EmailService(IOptions<EmailSettings> emailSettings, IVaultService vaultService)
        {
            this.emailSettings = emailSettings?.Value;
            this.vaultService = vaultService;
        }

        public async Task SendEmailFromTemplateAsync(EmailTemplate template, string to, string subject, dynamic templateData)
        {
            string templateId = "";
            switch (template)
            {
                case EmailTemplate.Registration:
                    templateId = emailSettings.RegistrationTemplate;
                    break;
                case EmailTemplate.ForgotPassword:
                    templateId = emailSettings.ForgotPasswordTemplate;
                    break;
            }

            var client = new SendGridClient((await vaultService.GetAuthSecrets()).SendgridApiKey);
            var message = new SendGridMessage
            {
                TemplateId = templateId,
                Subject = subject,
                From = new EmailAddress
                {
                    Email = emailSettings.Sender
                },
            };

            message.AddTo(new EmailAddress
            {
                Email = to,
            });
            message.SetTemplateData(templateData);
            var resp = await client.SendEmailAsync(message);
            Console.WriteLine(resp);
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
    }
}