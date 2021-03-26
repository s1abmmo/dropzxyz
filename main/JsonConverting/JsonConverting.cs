using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Windows;

namespace main.JsonConverting
{
    public class JsonConverting
    {
        public static string PrettyJson(string jsonString)
        {
            try
            {
                object jsondecode = DecodeJson(jsonString);
                string jsonencode = EncodeJsonBeauty(jsondecode);
                return jsonencode;
            }
            catch(Exception e)
            {
                return null;
            }
        }
        public static string EncodeJsonBeauty(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        public static string EncodeJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static object DecodeJson(string jsonstring)
        {
            return JsonConvert.DeserializeObject(jsonstring);
        }
        public static string EncodeJsonDropWindow(DropzWindow obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static DropzWindow DecodeJsonDropWindow(string jsonstring)
        {
            return JsonConvert.DeserializeObject<DropzWindow>(jsonstring);
        }
        public static Setting DecodeJsonSetting(string jsonstring)
        {
            return JsonConvert.DeserializeObject<Setting>(jsonstring);
        }

    }
}
