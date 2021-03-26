using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dropzwindow
{
    class JsonConverting
    {
        public static string PrettyJson(string jsonString)
        {
            try
            {
                object jsondecode = DecodeJson(jsonString);
                string jsonencode = JsonConvert.SerializeObject(jsondecode, Formatting.Indented);
                return jsonencode;
            }
            catch
            {
                return null;
            }
        }
        public static string EncodeJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        public static dynamic DecodeJson(string jsonstring)
        {
            return JsonConvert.DeserializeObject(jsonstring);
        }
        public static Setting DecodeJsonSetting(string jsonstring)
        {
            return JsonConvert.DeserializeObject<Setting>(jsonstring);
        }
        public static AutoScript DecodeJsonAutoScript(string jsonstring)
        {
            return JsonConvert.DeserializeObject<AutoScript>(jsonstring);
        }
        //public static object DecodeJsonAutoParameters(string jsonstring)
        //{
        //    return JsonConvert.DeserializeObject(jsonstring);
        //}

    }
}
