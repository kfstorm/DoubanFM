using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DoubanFM.Core.Json
{
    internal static class JsonHelper
    {
        public static T FromJson<T>(string json) where T : class
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (T)ser.ReadObject(stream);
            }
            catch
            {
                return null;
            }
        }
    }
}
