using System;
using Microsoft.AspNetCore.Identity;

namespace Pixelynx.Data.Entities
{
    public class UserEntity : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Credits { get; set; }
    }
}