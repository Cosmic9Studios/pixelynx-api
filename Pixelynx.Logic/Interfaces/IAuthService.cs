using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Pixelynx.Data.Entities;
using Pixelynx.Data.Interfaces;
using Pixelynx.Logic.Models;

namespace Pixelynx.Logic
{
    public interface IAuthService
    {
        Task<string> Login(string email, string password, string jwtSecret);
        Task<bool> Logout();
        Task<GenericResult<string>> Register(HttpRequest request, UserEntity newUser, string password);
        Task<GenericResult<string>> ConfirmEmail(IDbContextFactory dbContextFactory,
            string userId, string code, string type);
        Task<bool> ResendEmail(HttpRequest request, string userId, string email);
        Task<bool> ForgotPassword(HttpRequest request, string email);
        Task<GenericResult<string>> ResetPassword(string userId, string code, string newPassword);
        Task<GenericResult<string>> UpdatePassword(string userId, string oldPassword, string newPassword);
    }
}