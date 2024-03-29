using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pixelynx.Data.BlobStorage;

namespace Pixelynx.Tests.Mocks
{
    public class MockBlobStorage : IBlobStorage
    {
        private List<BlobObject> objects;

        public MockBlobStorage()
        {
            objects = new List<BlobObject>();
        }

        public Task<BlobObject> GetObject(string bucket, string objectPath)
        {
            return Task.FromResult(objects.FirstOrDefault(x => x.Key == objectPath));
        }

        public async Task<IEnumerable<BlobObject>> ListObjects(string bucket, string directory = "")
        {
            return await Task.Run(() => objects.Where(x => directory == "" || x.Key.StartsWith(directory)));
        }

        public async Task<bool> UploadFileToBucket(string bucket, string directory, string fileName, byte[] fileContent)
        {
            return await Task.Run(() =>
            {
                var key = $"{directory}/{fileName}";
                objects.Add(new BlobObject 
                {
                    Key = key,
                    Uri = $"http://assetstoretest.com/{key}"
                });
                
                return true;
            });
        }
    }
}