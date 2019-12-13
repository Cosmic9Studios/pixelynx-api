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
                    Uri = client.GetPreSignedURL(new Amazon.S3.Model.GetPreSignedUrlRequest
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

        /// <summary>
        /// Uploads a file to a specified bucket.
        /// </summary>
        /// <param name="bucket">The name of the bucket to place the file</param>
        /// <param name="file">The file data to be uploaded</param>
        /// <param name="file">The name and extension of the file being uploaded</param>
        public async Task<string> UploadFileToBucket(string bucket, string fileName, byte[] fileContent)
        {
            // TODO: This needs to be unique and specific to the model and easily
            // referenced to change the files in the directory at a later point. 
            // Probably based on a storage ID.
            var directory = fileName.Split('.').First();

            var request = new PutObjectRequest
            {
                BucketName = bucket,
                Key = $"{directory}/{fileName}"
            };

            using (var ms = new MemoryStream(fileContent))
            {
                request.InputStream = ms;

                var response = await client.PutObjectAsync(request);

                return "Upload Complete";
            }
        }
        #endregion
    }
}