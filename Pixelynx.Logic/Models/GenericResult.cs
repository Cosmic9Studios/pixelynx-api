using System.Collections.Generic;

namespace Pixelynx.Logic.Models
{
    public class GenericResult<T>
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
    }
}