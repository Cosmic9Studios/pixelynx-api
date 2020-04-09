using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace Pixelynx.Data.BlobStorage
{
    public class AmazonS3 : IBlobStorage
    {
        #region Fields.
        ///
        ///////////////////////////////////////////
        private AmazonS3Client client;
        #endregion

        #region Constructors
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
        public async Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "", bool signUrls = false)
        {
            var objectList = new List<BlobObject>();
            var listObjectsResponse = await client.ListObjectsAsync(bucket, directory);

            foreach (var obj in listObjectsResponse.S3Objects)
            {
                objectList.Add(new BlobObject
                {
                    Key = obj.Key,
                    Uri = signUrls ? client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
                    {
                        BucketName = bucket,
                        Key = obj.Key,
                        Expires = DateTime.Now.AddMinutes(5),
                        Protocol = Protocol.HTTP
                    }) : $"{client.Config.ServiceURL}/{bucket}/{obj.Key}"
                });
            }

            return objectList;
        }

        public async Task<bool> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent)
        {
            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = $"{directory}/{fileName}"
            };

            using (var ms = new MemoryStream(fileContent))
            {
                request.InputStream = ms;

                var response = await client.PutObjectAsync(request);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
        }
        #endregion
    }
}