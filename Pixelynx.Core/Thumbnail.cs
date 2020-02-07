using System;
using System.Linq;
using System.Net;

namespace Pixelynx.Core
{
    public class Thumbnail
    {
        protected Thumbnail(string name)
        {
            Name = name;
        }

        public Thumbnail(string name, string uri) : this(uri)
        {
            Uri = uri;
        }

        public Thumbnail(string name, byte[] rawData)
        {
            Name = name; 
            RawData = rawData;
        }

        public string Name { get; set; }
        public string Uri { get; set; } = string.Empty;
        public byte[] RawData { get; set; } = new byte[0];

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