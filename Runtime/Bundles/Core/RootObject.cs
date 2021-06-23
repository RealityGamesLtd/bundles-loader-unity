using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BundlesLoader.Bundles.Core
{
    public class Child
    {
        public string Name { get; set; }
        public List<Child> Childs { get; set; }
    }

    [JsonConverter(typeof(RootObjectConverter))]
    public class RootObject
    {
        public string RootName { get; set; }
        public List<Child> Childs { get; set; }
    }

    public class RootObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var root = JToken.Load(reader);

            RootObject obj = new RootObject()
            {
                RootName = root["RootName"].Value<string>(),
                Childs = new List<Child>()
            };

            var childs = root["Names"].Value<JArray>();
            obj.Childs.AddRange(GetChildren(childs));
            return obj;
        }

        private IEnumerable<Child> GetChildren(JArray childs)
        {
            List<Child> ret = new List<Child>();

            foreach(JToken token in childs)
            {
                var name = token["Name"].Value<string>();

                if(token["Childs"].Type != JTokenType.Null)
                {
                    var chlds = token["Childs"].Value<JArray>();
                    Child chld = new Child()
                    {
                        Name = name,
                        Childs = token["Childs"] != null ? new List<Child>() : null
                    };
                    ret.Add(chld);
                    chld.Childs.AddRange(GetChildren(chlds));
                }
                else
                {
                    Child chld = new Child()
                    {
                        Name = name,
                        Childs = null
                    };
                    ret.Add(chld);
                }
            }

            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {}
    }
}
