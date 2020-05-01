using System.Collections.Generic;

namespace Pixelynx.Api.Responses
{
    public class DownloadResponse
    {
        public List<Core.Asset> Models { get; set; }
        public List<Core.Asset> Animations { get; set; }
        public string Name { get; set; }
    }
}