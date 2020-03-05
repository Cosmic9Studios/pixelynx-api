using System;
using System.Security.Cryptography;

namespace Pixelynx.Core.Helpers
{
    public static class CrytoHelper
    {
        public static string GenerateHash(this byte[] byteArray)
        {
            // Why SHA1? -- https://stackoverflow.com/a/2640600/1772419
            using(SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                return Convert.ToBase64String(sha1.ComputeHash(byteArray));
            }
        }
    }
}

