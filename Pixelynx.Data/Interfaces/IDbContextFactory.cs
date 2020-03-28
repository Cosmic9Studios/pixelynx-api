namespace Pixelynx.Data.Interfaces
{
    public interface IDbContextFactory
    {
        PixelynxContext Create();
    }
}