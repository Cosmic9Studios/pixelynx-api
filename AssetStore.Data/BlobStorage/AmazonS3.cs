using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace AssetStore.Data.BlobStorage
{
    public class AmazonS3 : IBlobStorage
    {
        #region Fields.
        ///
        ///////////////////////////////////////////
        private AmazonS3Client client;
        #endregion

        #region Constructors
        /// <summary>
        /// The instance of the <cref="AmazonS3" /> class.
        /// </summary>
        /// <param name="address">The address of the s3 server</param>
        /// <param name="accessKey">The s3 access key</param>
        /// <param name="secretKey">The s3 secret key</param>
        public AmazonS3(string address, string accessKey, string secretKey)
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                ServiceURL = address,
                ForcePathStyle = true
            };
            
            client = new AmazonS3Client(accessKey, secretKey, config); 
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Lists all the objects in the bucket.
        /// </summary>
        /// <param name="bucket">The name of the bucket to parse</param>
        public async Task<IEnumerable<BlobObject>> ListObjects(string bucket)
        {
            var objectList = new List<BlobObject>();
            var listObjectsResponse = await client.ListObjectsAsync(bucket);

            foreach (var obj in listObjectsResponse.S3Objects)
            {
                objectList.Add(new BlobObject 
                {
                    Key = obj.Key,
                    Uri = client.GetPreSignedURL(new GetPreSignedUrlRequest 
                    {
                        BucketName = bucket,
                        Key = obj.Key,
                        Expires = DateTime.Now.AddMinutes(5),
                        Protocol = Protocol.HTTP
                    })
                });
            }

            return objectList;
        }
        #endregion
    }
}