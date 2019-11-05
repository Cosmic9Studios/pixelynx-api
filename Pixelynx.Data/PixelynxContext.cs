using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pixelynx.Data.Entities;

namespace Pixelynx.Data
{
    public class PixelynxContext : DbContext
    {
        #region Constructors.
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelynxContext" /> class.
        /// </summary>
        public PixelynxContext(DbContextOptions<PixelynxContext> options) : base(options)
        {
        }
        #endregion

        #region DbSets.
        public DbSet<UserEntity> Users { get; set; }
        #endregion
    }
}