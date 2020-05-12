using Pixelynx.Data.Interfaces;
using Pixelynx.Data.Repositories;

namespace Pixelynx.Data.Models
{
    public class UnitOfWork
    {
        public UnitOfWork(IDbContextFactory dbContextFactory)
        {
            AssetRepository = new AssetRepository(dbContextFactory);
            PaymentRepository = new PaymentRepository(dbContextFactory);
        }

        public AssetRepository AssetRepository { get; }
        public PaymentRepository PaymentRepository { get; }
    }
}