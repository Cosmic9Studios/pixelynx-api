using System.Threading.Tasks;

namespace Pixelynx.Data.Interfaces
{
    public interface IDbContextFactory
    {
        PixelynxContext CreateAdmin();
        PixelynxContext CreateRead();
        PixelynxContext CreateWrite();
        PixelynxContext CreateReadWrite();
    }
}