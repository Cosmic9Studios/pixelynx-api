using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pixelynx.Api.Requests
{
    public class UploadRequest
    {
        [FromForm(Name="data")]
        public IFormFile Data { get; set; }

        [FromForm(Name="name")]
        public string Name { get; set; }

        [FromForm(Name="parentId")]
        public string ParentId { get; set; }

        [FromForm(Name="type")]
        public string Type { get; set; }
    }
}