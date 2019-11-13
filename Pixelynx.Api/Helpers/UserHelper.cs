using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MoreLinq;
using Pixelynx.Data.Entities;

namespace Pixelynx.Api.Helpers
{
    public static class UserHelper
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
    }
}