using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AssetStore.Api.Settings
{
    public class AccountSettings 
    {
        public Dictionary<string, string> KeyFile { get; set; }
    }
}