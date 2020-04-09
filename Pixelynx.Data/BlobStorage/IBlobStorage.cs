using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixelynx.Data.BlobStorage
{
    public interface IBlobStorage
    {
        Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "", bool signUrls = false);
        Task<bool> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent);
    }
}
