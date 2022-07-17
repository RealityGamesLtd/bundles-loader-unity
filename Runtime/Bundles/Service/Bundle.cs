using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BundlesLoader.Service
{
    public class Bundle
    {
        public System.Action<Bundle> OnAssetsChanged;

        public Dictionary<string, Object> Assets { get; private set; }
        public AssetBundle AssetBundle { get; private set; }
        public string Name { get; private set; }
        public string Hash { get; private set; }

        public Bundle(AssetBundle bundle, string name, string hash)
        {
            Assets = new Dictionary<string, Object>();
            AssetBundle = bundle;
            Name = name;
            Hash = hash;
        }

        public void Update(Bundle bundle)
        {
            Hash = bundle.Hash;
            Assets.Clear();
            AssetBundle = bundle.AssetBundle;

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

            if (Assets.TryGetValue(name, out var elem))
            {
                if (elem is T t) return t;
            }

            if (!string.IsNullOrEmpty(name))
            {
                var res = AssetBundle.LoadAsset<T>(assetName);
                Assets.Add(name, res);
                return res;
            }

            return null;
        }
    }
}
