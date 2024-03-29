using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MoreLinq;
using Pixelynx.Data.Entities;
using static AuthService;

namespace Pixelynx.Logic.Extensions
{
    public static class UserExtensions
    {
        public static async Task<string> GenerateToken(this UserEntity user, UserManager<UserEntity> userManager, string jwtSecret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var defaultClaims = new List<Claim> { new Claim(ClaimTypes.Name, user.Id.ToString()) };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(defaultClaims.Concat(await userManager.GetClaimsAsync(user)).DistinctBy(x => x.Type)),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateConfirmationUrl(this UserEntity user, HttpRequest request, string code, ConfirmationType confirmationType)
        {
            RequestHeaders header = request.GetTypedHeaders();
            var fullUri = header.Referer.AbsoluteUri;
            var authority = header.Referer.Authority;

            var baseUrl = fullUri.Substring(0, fullUri.IndexOf(authority) +  authority.Length);
            return $"{baseUrl}/account/confirm?userId={user.Id}&code={code}&type={confirmationType.ToString()}";
        }
    }
}