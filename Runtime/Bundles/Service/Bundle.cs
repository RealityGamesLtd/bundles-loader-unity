using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace BundlesLoader.Service
{
    public class Bundle
    {
        public List<Object> Assets { get; private set; }
        public string Name { get; private set; }
        public string Hash { get; private set; }
        public System.Action<Bundle> OnAssetsChanged { get; set; }

        public Bundle(Object[] assets, string name, string hash)
        {
            Assets = assets.ToList();
            Name = name;
            Hash = hash;
        }

        public void Update(List<Object> objects, string hash)
        {
            Assets = objects;
            Hash = hash;

            OnAssetsChanged?.Invoke(this);
        }

        public override string ToString()
        {
            return $"{Name}, hash: {Hash}";
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            var extension = Path.GetExtension(assetName);
            string name = assetName;

            if (!string.IsNullOrEmpty(extension))
            {
                var textureMatch = Regex.Match(extension, AssetsRegexs.TEXTURE_REGEX);
                var byteMatch = Regex.Match(extension, AssetsRegexs.BYTE_REGEX);
                var atlasMatch = Regex.Match(extension, AssetsRegexs.SPRITEATLAS_REGEX);

                if ((textureMatch != null && textureMatch.Success) ||
                    (byteMatch != null && byteMatch.Success) ||
                    (atlasMatch != null && atlasMatch.Success))
                {
                    name = Path.GetFileNameWithoutExtension(name);
                }
            }

            if (!string.IsNullOrEmpty(name))
            {
                var asset = Assets.Find(x => x is T && x.name.Equals(name));
                if (asset != null)
                {
                    return asset as T;
                }
            }

            return null;
        }
    }
}
