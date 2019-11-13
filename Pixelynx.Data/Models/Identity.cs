using System;
using Microsoft.AspNetCore.Identity;

namespace Pixelynx.Data.Models
{
    public class Role : IdentityRole<Guid>
    {
    }

    public class RoleClaim : IdentityRoleClaim<Guid>
    {
    }

    public class UserClaim : IdentityUserClaim<Guid>
    {
    }

    public class UserLogin : IdentityUserLogin<Guid>
    {
    }

    public class UserRole : IdentityUserRole<Guid>
    {
    }

    public class UserToken : IdentityUserToken<Guid>
    {
    }
}