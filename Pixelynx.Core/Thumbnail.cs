using System;
using System.Linq;
using System.Net;

namespace Pixelynx.Core
{
    public class Thumbnail
    {
        public Thumbnail(string uri)
        {
            Uri = uri;
        }

        public Thumbnail(byte[] rawData, string filename)
        {
            RawData = rawData;
            FileName = filename;
        }

        public string Uri { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = new byte[0];
        public string FileName { get; }

        public byte[] GetRawData()
        {
            if (!RawData.Any() && !string.IsNullOrWhiteSpace(Uri))
            {
                using (var webClient = new WebClient()) 
                { 
                    RawData = webClient.DownloadData(Uri);
                }
            }

            return RawData;
        }
    }
}