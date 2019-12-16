using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixelynx.Data.BlobStorage
{
    public interface IBlobStorage
    {
        /// <summary>
        /// Lists all the objects in the bucket.
        /// </summary>
        /// <param name="bucket">The name of the bucket to parse</param>
        Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "");

        /// <summary>
        /// Uploads a file to a specified bucket.
        /// </summary>
        /// <param name="bucket">The name of the bucket to place the file</param>
        /// <param name="directory">The directory to store the file</param>
        /// <param name="fileName">The name and extension of the file being uploaded</param>
        /// <param name="fileContent">The file data to be uploaded</param>
        Task<string> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent);
    }
}
