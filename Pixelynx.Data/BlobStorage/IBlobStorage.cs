using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixelynx.Data.BlobStorage
{
    public interface IBlobStorage
    {
        Task<BlobObject> GetObject(string bucket, string objectPath);
        Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "");
        Task<bool> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent);
    }
}
