using System;
using System.Collections.Generic;

namespace Pixelynx.Core.Helpers
{
    public static class DictionaryHelpers
    {
        public static T ToObject<T>(this Dictionary<string, object> dict, Type type = null)
        {
            if (type == null)
            {
                type = typeof(T);
            }
            
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if(prop == null) continue;

                object value = kv.Value;
                if (value is Dictionary<string, object>)
                {
                    value = ToObject<T>((Dictionary<string, object>) value, prop.PropertyType); // <= This line
                }

                prop.SetValue(obj, value, null);
            }

            return (T)obj;
        }
    }
}