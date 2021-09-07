using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bundles.Utils
{
    public class Child
    {
        public string Name { get; set; }
        public List<Child> Children { get; set; }
    }

    [JsonConverter(typeof(RootObjectConverter))]
    public class RootObject
    {
        public string RootName { get; set; }
        public List<Child> Children { get; set; }
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
                Children = new List<Child>()
            };

            var childs = root["Names"].Value<JArray>();
            obj.Children.AddRange(GetChildren(childs));
            return obj;
        }

        private IEnumerable<Child> GetChildren(JArray childs)
        {
            List<Child> ret = new List<Child>();

            foreach(JToken token in childs)
            {
                var name = token["Name"].Value<string>();

                if(token["Children"].Type != JTokenType.Null)
                {
                    var chlds = token["Children"].Value<JArray>();
                    Child chld = new Child()
                    {
                        Name = name,
                        Children = token["Children"] != null ? new List<Child>() : null
                    };
                    ret.Add(chld);
                    chld.Children.AddRange(GetChildren(chlds));
                }
                else
                {
                    Child chld = new Child()
                    {
                        Name = name,
                        Children = null
                    };
                    ret.Add(chld);
                }
            }

            return ret;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {}
    }
}
