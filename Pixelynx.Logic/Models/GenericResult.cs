using System.Collections.Generic;

namespace Pixelynx.Logic.Models
{
    public class GenericResult<T> : GenericResult
    {
        public T Data { get; set; }
    }

    public class GenericResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
    }
}