using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

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
            var name = Path.GetFileNameWithoutExtension(assetName);
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
