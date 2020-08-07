using System.Threading.Tasks;
using Pixelynx.Logic.Model.Email;
using Pixelynx.Logic.Services;

namespace Pixelynx.Logic.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailFromTemplateAsync(EmailTemplate template, string to, string subject, EmailData templateData);
        Task SendEmailAsync(string to, string subject, string body);
    }
}