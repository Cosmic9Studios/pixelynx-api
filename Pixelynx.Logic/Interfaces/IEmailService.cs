using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixelynx.Logic.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailFromTemplateAsync(string to, string subject, string templateName, Dictionary<string, string> variables);
    }
}