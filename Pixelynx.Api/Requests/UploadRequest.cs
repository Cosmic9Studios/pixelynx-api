using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pixelynx.Api.Requests
{
    public class UploadRequest
    {
        public IFormCollection Form { get; set; }

        [FromForm(Name="parentId")]
        public string ParentId { get; set; }
    }
}