using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace AssetStore.Data.BlobStorage
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
        /// <summary>
        /// Initializes a new instance of the <cref="GCStorage"/> class.
        /// </summary>
        public GCStorage()
        {
            client = StorageClient.Create();
            
            var scopes = new string[] { "https://www.googleapis.com/auth/cloud-platform" };
            var cred = GoogleCredential
                 .GetApplicationDefault()
                 .CreateScoped(scopes)
                 .UnderlyingCredential as ServiceAccountCredential;
            
            urlSigner = UrlSigner.FromServiceAccountCredential(cred);
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Lists all the objects in the bucket.
        /// </summary>
        /// <param name="bucket">The name of the bucket to parse</param>
        public async Task<IEnumerable<BlobObject>> ListObjects(string bucket)
        {
            return await Task.Run(() => client.ListObjects(bucket).Select(x => new BlobObject
            {
                Key = x.Name, 
                Uri = urlSigner.Sign(bucket, x.Name, TimeSpan.FromHours(1), HttpMethod.Get)
            }));
        }
        #endregion
    }
}