using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Pixelynx.Data.Entities
{
    public class UserEntity : IdentityUser<Guid>
    {
    }
}