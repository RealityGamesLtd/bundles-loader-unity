using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace BundlesLoader.Service
{
    public class Bundle
    {
        public System.Action<Bundle> OnAssetsChanged;

        public Dictionary<string, Object> Assets { get; private set; } = new Dictionary<string, Object>();
        public string Name { get; private set; }
        public string Hash { get; private set; }

        public Bundle(Object[] assets, string name, string hash)
        {
            Refresh(assets.ToList());
            Name = name;
            Hash = hash;
        }

        public void Update(Dictionary<string, Object> objects, string hash)
        {
            Refresh(objects);
            Hash = hash;

            OnAssetsChanged?.Invoke(this);
        }

        private void Refresh(List<Object> objects)
        {
            for (int i = 0; i < objects.Count; ++i)
            {
                AddEntry(objects[i]);
            }
        }

        private void Refresh(Dictionary<string, Object> objects)
        {
            foreach(var pair in  objects)
            {
                AddEntry(pair.Value);
            }
        }

        private void AddEntry(Object obj)
        {
            var name = obj.name;
            if (Assets.TryGetValue(name, out var asset))
            {
                if (asset != null)
                    Object.Destroy(asset);
                Assets[name] = obj;
            }
            else
            {
                Assets.Add(name, obj);
            }
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
                Object asset = null;

                if (Assets.TryGetValue(name, out var elem))
                {
                    if (elem is T) asset = elem;
                }

                if (asset != null)
                {
                    return asset as T;
                }
            }

            return null;
        }
    }
}
