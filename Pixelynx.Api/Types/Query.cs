using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixelynx.Data.BlobStorage;

namespace Pixelynx.Api.Types
{
    public class Query
    {
        #region Fields. 
        ///
        /////////////////////////////////////
        private IBlobStorage blobStorage;
        #endregion

        public Query(IBlobStorage blobStorage)
        {
            this.blobStorage = blobStorage;
        }

        public string Hello() => "world"; 

        public async Task<List<Asset>> Assets(string filter) =>
            (await blobStorage.ListObjects("c9s-assetstore"))
                .GroupBy(x => x.Key.Split('/')[0])
                .Where(x => string.IsNullOrWhiteSpace(filter) || x.Key.Contains(filter))
                .Select(x => new Asset 
                {
                    Uri = x.FirstOrDefault(y => y.Key.EndsWith(".glb")).Uri,
                    ThumbnailUri = x.FirstOrDefault(y => !y.Key.EndsWith("glb") && y.Key != $"{x.Key}/").Uri,
                    Name = x.Key,
                }).ToList();
    }
}