using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVCBlazorChatApp.Server.Helpers.StatusMessages
{
    public static class TempDataStatusMessageExtension
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize<T>(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object val;

            tempData.TryGetValue(key, out val);

            return val == null ? null : JsonSerializer.Deserialize<T>((string)val);
        }
    }
}
