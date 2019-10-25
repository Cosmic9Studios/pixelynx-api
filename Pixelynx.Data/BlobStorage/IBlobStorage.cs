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
        Task<IEnumerable<BlobObject>> ListObjects(string bucket);
    }
}
