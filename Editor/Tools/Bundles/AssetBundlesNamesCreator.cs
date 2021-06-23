using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public static class AssetBundlesNamesCreator
    {
        public class Child
        {
            public string Name { get; set; }
            public List<Child> Childs { get; set; }
        }

        public class RootObject
        {
            public string RootName { get; set; }
            public List<Child> Names { get; set; }
        }

        public static string CreateNames(List<Container> objects)
        {
            RootObject root = new RootObject() { RootName = "Bundles", Names = new List<Child>() };

            foreach(var obj in objects)
            {
                Child basChild = new Child() { Name = obj.BundleName, Childs = new List<Child>() };
                foreach(var slctObj in obj.SelectedObjects)
                {
                    var names = GenerateNamesFromAsset(slctObj);
                    var extension = Path.GetExtension(AssetDatabase.GetAssetPath(slctObj));
                    var currChild = new Child()
                    {
                        Name = $"{slctObj.name.Replace("(Clone)", string.Empty)}{extension}",
                        Childs = names != null ? new List<Child>() : null
                    };

                    basChild.Childs.Add(currChild);
                    if (names != null)
                        currChild.Childs.AddRange(names.Select(x => new Child() { Name = x, Childs = null }));
                }
                root.Names.Add(basChild);
            }

            return JsonConvert.SerializeObject(root, Formatting.Indented);
        }

        private static string[] GenerateNamesFromAsset(Object slctObj)
        {
            //TODO: Add more types
            var type = slctObj.GetType();
            if(type == typeof(SpriteAtlas))
            {
                var spriteAtlas = slctObj as SpriteAtlas;
                var sprites = new Sprite[spriteAtlas.spriteCount];
                spriteAtlas.GetSprites(sprites);
                return sprites.Select(x => x.name.Replace("(Clone)", string.Empty)).ToArray();
            }
            else
            {
                return null;
            }
        }
    }
}
