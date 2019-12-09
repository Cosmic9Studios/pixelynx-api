using System.Collections.Generic;

namespace Pixelynx.Logic.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
        void SendEmailFromTemplate(string to, string subject, string templateName, Dictionary<string, string> variables);
    }
}