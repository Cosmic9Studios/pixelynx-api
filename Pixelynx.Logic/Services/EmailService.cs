using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Interfaces;
using Pixelynx.Logic.Model.Email;
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

        public async Task SendEmailFromTemplateAsync(EmailTemplate template, string to, string subject, EmailData templateData)
        {
            var apiKey = (await vaultService.GetAuthSecrets()).SendInBlueApiKey;
            Configuration.Default.AddApiKey("api-key", apiKey);
            var apiInstance = new SMTPApi();
           
            var sendSmtpEmail = new SendSmtpEmail(
                new SendSmtpEmailSender("Pixelynx", emailSettings.Sender),
                new List<SendSmtpEmailTo>{new SendSmtpEmailTo(to, templateData.Receipient)}
            );

            sendSmtpEmail.Params = templateData;
            switch (template)
            {
                case EmailTemplate.Registration:
                    sendSmtpEmail.TemplateId = long.Parse(emailSettings.RegistrationTemplate);
                    break;
                case EmailTemplate.ForgotPassword:
                    sendSmtpEmail.TemplateId = long.Parse(emailSettings.ForgotPasswordTemplate);
                    break;
            }
            try
            {
                // Send a template
                // SendTemplateEmail result = apiInstance.SendTemplate(templateId, sendEmail);
                var result = apiInstance.SendTransacEmail(sendSmtpEmail);
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