using System;
using System.Threading.Tasks;
using Pixelynx.Data;
using Pixelynx.Data.Entities;

namespace Pixelynx.Tests.Builders
{
    public class UserBuilder
    {
        private UserEntity user;
        private readonly PixelynxContext context;
        public UserBuilder(PixelynxContext context) => this.context = context;
        
        public UserBuilder New(string username)
        {
            user = new UserEntity()
            {
                Id = Guid.NewGuid(),
                UserName = username
            };

            return this;
        }

        public UserBuilder WithCredits(int credits)
        {
            user.Credits = credits;
            return this;
        }

        public async Task<UserEntity> BuildAndInsert()
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            return user;
        }
    }
}
