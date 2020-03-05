using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Pixelynx.Api.Requests
{
    public class ExamineAssetRequest
    {
        public List<IFormFile> Data { get; set; }
    }
}