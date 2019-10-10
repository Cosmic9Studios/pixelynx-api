using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetStore.Api.Settings;
using AssetStore.Data.BlobStorage;
using Microsoft.Extensions.Options;

namespace AssetStore.Api.Types
{
    public class Query
    {
        #region Fields. 
        ///
        /////////////////////////////////////
        private IBlobStorage blobStorage;

        private string _minioRootPath;
        #endregion

        public Query(IBlobStorage blobStorage, IOptions<MinioSettings> minioSettings)
        {
            this.blobStorage = blobStorage;
            _minioRootPath = minioSettings.Value.RootPath;
        }

        public string Hello() => "world"; 

        public async Task<List<Asset>> Assets()
        {
            return (await blobStorage.ListObjects(_minioRootPath))
                .GroupBy(x => x.Key.Split('/')[0])
                .Select(x => new Asset 
                {
                    Uri = x.FirstOrDefault(y => y.Key.EndsWith(".glb"))?.Uri,
                    ThumbnailUri = x.FirstOrDefault(y => !y.Key.EndsWith("glb") && y.Key != $"{x.Key}/")?.Uri,
                    Name = x.Key,
                }).ToList();
        }
    }
}