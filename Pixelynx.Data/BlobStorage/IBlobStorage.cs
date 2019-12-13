using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace Pixelynx.Data.BlobStorage
{
  public interface IBlobStorage
  {
    /// <summary>
    /// Lists all the objects in the bucket.
    /// </summary>
    /// <param name="bucket">The name of the bucket to parse</param>
    Task<IEnumerable<BlobObject>> ListObjects(string bucket);

    Task<string> UploadFileToBucket(string bucket, string fileName, byte[] fileContent);
  }
}
