using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using AssetStore.Data.BlobStorage;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using HotChocolate.Types;

namespace AssetStore.Api.Types
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

        public async Task<List<Asset>> Assets()
        {
            return (await blobStorage.ListObjects("c9s-assetstore"))
                .GroupBy(x => x.Key.Split('/')[0])
                .Select(x => new Asset 
                {
                    Uri = x.FirstOrDefault(y => y.Key.EndsWith(".glb")).Uri,
                    ThumbnailUri = x.FirstOrDefault(y => !y.Key.EndsWith("glb") && y.Key != $"{x.Key}/").Uri,
                    Name = x.Key,
                }).ToList();
        }
    }
}