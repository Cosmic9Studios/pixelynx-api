using System.Threading.Tasks;
using Pixelynx.Logic.Services;

namespace Pixelynx.Logic.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailFromTemplateAsync(EmailTemplate template, string to, string subject, dynamic templateData);
        Task SendEmailAsync(string to, string subject, string body);
    }
}