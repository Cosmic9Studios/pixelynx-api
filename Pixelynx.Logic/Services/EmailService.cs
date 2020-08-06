using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Settings;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;

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
            var apiKey = (await vaultService.GetAuthSecrets()).SendInBlueApiKey;
            Configuration.Default.AddApiKey("api-key", apiKey);
            var apiInstance = new SMTPApi();
            long templateId = 1;
            var sendEmail = new SendEmail(new List<string>{ to });
            
            switch (template)
            {
                case EmailTemplate.Registration:
                    templateId = long.Parse(emailSettings.RegistrationTemplate);
                    break;
                case EmailTemplate.ForgotPassword:
                    templateId = long.Parse(emailSettings.ForgotPasswordTemplate);
                    break;
            }
            try
            {
                // Send a template
                SendTemplateEmail result = apiInstance.SendTemplate(templateId, sendEmail);
                Debug.WriteLine(result);
            }
            catch (Exception e)
            {
                Debug.Print("Exception when calling SMTPApi.SendTemplate: " + e.Message );
            }
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