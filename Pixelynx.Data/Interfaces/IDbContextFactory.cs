using System.Threading.Tasks;

namespace Pixelynx.Data.Interfaces
{
    public interface IDbContextFactory
    {
        PixelynxContext Create();
    }
}