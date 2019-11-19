using System;

namespace Pixelynx.Api.Responses
{
    public class LoginResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}