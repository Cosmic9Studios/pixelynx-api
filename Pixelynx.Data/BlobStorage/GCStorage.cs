using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using System.IO;

namespace Pixelynx.Data.BlobStorage
{
    public class GCStorage : IBlobStorage
    {
        #region Fields. 
        ///
        //////////////////////////////////////////
        private StorageClient client;
        private UrlSigner urlSigner;
        #endregion

        #region Constructors.
        public GCStorage(UrlSigner urlSigner)
        {
            client = StorageClient.Create();
            this.urlSigner = urlSigner;
        }
        #endregion

        #region Public methods.
        public async Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "")
        {
            return await Task.Run(() => client.ListObjects(bucket, directory).Select(x => new BlobObject
            {
                Key = x.Name,
                Uri = urlSigner.Sign(bucket, x.Name, TimeSpan.FromHours(1), HttpMethod.Get)
            }));
        }

        public async Task<bool> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent)
        {
            using (var ms = new MemoryStream(fileContent))
            {
                var response = await client.UploadObjectAsync(bucket, $"{directory}/{fileName}", null, ms);
                return !string.IsNullOrWhiteSpace(response.Id);
            }
        }
        #endregion
    }
}