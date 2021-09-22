using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace bot_2.Json
{
    public class JsonCommunicator
    {
        string directory = "";
        public JsonCommunicator()
        {
            directory = Directory.GetCurrentDirectory();
        }

        public string ToString(JsonRecord record)
        {
            string json = JsonConvert.SerializeObject(record, Formatting.Indented);
            return json;
        }
        public void ModifyFile(string file, string contents)
        {
            string path = directory + @"\" + @file + @".json";
            if (File.Exists(path))
            {
                File.WriteAllText(path, contents);
            }
            else
            {

                //File.Create(path);
                File.WriteAllText(path, contents);
            }
        }
        public void OverwriteFile(string key, string value)
        {
            string path = directory + @"\" + @"registration.json";
            if (File.Exists(path))
            {
                string stuff = File.ReadAllText(path);
                var obj = (JObject)JsonConvert.DeserializeObject(stuff);
                obj[key] = value;

                string reserialize = JsonConvert.SerializeObject(obj, Formatting.Indented);
                File.WriteAllText(path, reserialize);
            }
        }

        public ulong GetValue(string file, string key)
        {
            string path = directory + @"\" + @file + @".json";
            if (File.Exists(path))
            {
                string stuff = File.ReadAllText(path);
                var obj = JObject.Parse(stuff); //(JObject)JsonConvert.DeserializeObject(stuff);
                var value = obj[key];

                return (ulong)value;
            }
            return 0;
        }

        public string GetStringValue(string file, string key)
        {
            string path = directory + @"\" + @file + @".json";
            if (File.Exists(path))
            {
                string stuff = File.ReadAllText(path);
                var obj = JObject.Parse(stuff); //(JObject)JsonConvert.DeserializeObject(stuff);
                var value = obj[key];

                return (string)stuff;
            }
            return "";
        }

    }

}
