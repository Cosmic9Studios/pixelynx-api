using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pixelynx.Api.Requests
{
    public class UploadMeshRequest
    {
        [FromForm(Name="asset")]
        public IFormFile Asset { get; set; }

        [FromForm(Name="name")]
        public string Name { get; set; }
    }
}