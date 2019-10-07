using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Cloud.Kms.V1;
using Google.Protobuf;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace AssetStore.Api.Helpers
{
    public class EncryptedFileProvider : IFileProvider
    {
        private readonly KeyManagementServiceClient kms;
        private readonly IFileProvider innerProvider;

        /// <summary>
        /// Creates an EncryptedFileProvider.
        /// </summary>
        /// <param name="kms">Optional.</param>
        /// <param name="innerProvider">Optional.  An IFileProvider
        /// containing encrypted files.</param>
        public EncryptedFileProvider(
            KeyManagementServiceClient kms = null,
            IFileProvider innerProvider = null)
        {
            this.kms = kms ?? KeyManagementServiceClient.Create();
            if (innerProvider == null)
            {
                string fullPath = System.Reflection.Assembly
                    .GetAssembly(typeof(EncryptedFileProvider)).Location;
                string directory = Path.GetDirectoryName(fullPath);
                this.innerProvider = new PhysicalFileProvider(directory);
            }
            else
            {
                this.innerProvider = innerProvider;
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var innerContents = innerProvider.GetDirectoryContents(subpath);
            if (innerContents == null)
            {
                return null;
            }
            return new EncryptedDirectoryContents(kms, innerContents);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return EncryptedFileInfo.FromFileInfo(kms,
                innerProvider.GetFileInfo(subpath),
                innerProvider.GetFileInfo(Path.ChangeExtension(subpath, ".keyname")));
        }

        public IChangeToken Watch(string filter)
        {
            return innerProvider.Watch(filter);
        }
    }

    public class EncryptedFileInfo : IFileInfo
    {
        private readonly KeyManagementServiceClient kms;
        private readonly IFileInfo keynameFileInfo;
        private readonly Lazy<CryptoKeyName> cryptoKeyName; 
        private readonly IFileInfo innerFileInfo;
        public static IFileInfo FromFileInfo(KeyManagementServiceClient kms,
            IFileInfo fileInfo, IFileInfo keynameFileInfo)
        {
            if (fileInfo == null)
            {
                return null;
            }
            if (fileInfo.IsDirectory)
            {
                return fileInfo;
            }
            if (!fileInfo.Name.EndsWith(".encrypted")) 
            {
                return null;
            }
            return new EncryptedFileInfo(kms, fileInfo, keynameFileInfo);
        }

        private EncryptedFileInfo(KeyManagementServiceClient kms,
            IFileInfo innerFileInfo, IFileInfo keynameFileInfo)
        {
            this.kms = kms;
            this.keynameFileInfo = keynameFileInfo;
            this.innerFileInfo = innerFileInfo;
            this.cryptoKeyName = new Lazy<CryptoKeyName>(() => UnpackKeyName(keynameFileInfo));
        }

        public bool Exists => innerFileInfo.Exists && innerFileInfo.Name.EndsWith(".encrypted");

        public long Length => CreateReadStream().Length;

        public string PhysicalPath => null;

        public string Name => innerFileInfo.Name;

        public DateTimeOffset LastModified => innerFileInfo.LastModified;

        public bool IsDirectory => innerFileInfo.IsDirectory;

        public Stream CreateReadStream()
        {
            if (!Exists)
            {
                throw new FileNotFoundException(innerFileInfo.Name);
            }

            DecryptResponse response;
            using (var stream = innerFileInfo.CreateReadStream())
            {
                response = kms.Decrypt(cryptoKeyName.Value,
                    ByteString.FromStream(stream));
            }
            MemoryStream memStream = new MemoryStream();
            response.Plaintext.WriteTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        private static CryptoKeyName UnpackKeyName(IFileInfo keynameFileInfo)
        {
            if (keynameFileInfo == null || !keynameFileInfo.Exists || keynameFileInfo.IsDirectory)
            {
                throw new FileNotFoundException("Encrypted file found, but "
                    + "failed to find corresponding keyname file.",
                    keynameFileInfo.Name);
            }

            using (var reader = new StreamReader(keynameFileInfo.CreateReadStream()))
            {
                string line = "";
                while (!reader.EndOfStream) 
                {
                    line = reader.ReadLine().Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) 
                    {
                        continue; // blank or comment;
                    }
                    var keyName = CryptoKeyName.Parse(line);
                    if (keyName != null)
                    {
                        return keyName;
                    }
                    break;
                }
                throw new Exception(
                    $"Incorrectly formatted keyname file {keynameFileInfo.Name}.\n" +
                    "Expected projects/<projectId>/locations/<locationId>/keyRings/<keyringId>/cryptoKeys/<keyId>\n" +
                    $"Instead, found {line}.");                        
            }
        }
    }

    public class EncryptedDirectoryContents : IDirectoryContents
    {
        private readonly KeyManagementServiceClient kms;
        private readonly IDirectoryContents innerDirectoryContents;
        public EncryptedDirectoryContents(KeyManagementServiceClient kms,
            IDirectoryContents innerDirectoryContents)
        {
            this.kms = kms;
            this.innerDirectoryContents = innerDirectoryContents;
        }

        public bool Exists => innerDirectoryContents.Exists;

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            // Only enumerate directories, and files with matching
            // .encrypted and .keyname extensions.
            var keynameFiles = new Dictionary<String, IFileInfo>();
            var encryptedFiles = new List<IFileInfo>();
            foreach (var fileInfo in innerDirectoryContents)
            {
                if (fileInfo.IsDirectory)
                {
                    yield return fileInfo;
                }
                else
                {
                    string extension = Path.GetExtension(fileInfo.Name);
                    if (extension == ".encrypted")
                    {
                        encryptedFiles.Add(fileInfo);
                    }
                    else if (extension == ".keyname")
                    {
                        keynameFiles[fileInfo.Name] = fileInfo;
                    }
                }
            }
            foreach (IFileInfo fileInfo in encryptedFiles) 
            {
                IFileInfo keynameFile = keynameFiles.GetValueOrDefault(
                    Path.ChangeExtension(fileInfo.Name, ".keyname"));
                if (keynameFile != null)
                {
                    yield return EncryptedFileInfo.FromFileInfo(kms, fileInfo,
                        keynameFile);
                } 
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            IEnumerator<IFileInfo> x = this.GetEnumerator();
            return x;
        }
    }
}