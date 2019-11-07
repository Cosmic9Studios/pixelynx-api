using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pixelynx.Api.Settings;
using Pixelynx.Data.BlobStorage;

namespace Pixelynx.Api.Types
{
    public class Query
    {
        #region Fields. 
        ///
        /////////////////////////////////////
        private IBlobStorage blobStorage;
        private AssetstoreSettings assetstoreSettings;
        #endregion

        public Query(IBlobStorage blobStorage, IOptions<AssetstoreSettings> assetstoreSettings)
        {
            this.blobStorage = blobStorage;
            this.assetstoreSettings = assetstoreSettings.Value;
        }

        public string Hello() => "world"; 

        public async Task<List<Asset>> Assets(string filter) =>
            (await blobStorage.ListObjects(assetstoreSettings.BucketName))
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