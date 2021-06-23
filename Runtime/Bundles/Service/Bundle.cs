using UnityEngine;

namespace BundlesLoader.Service
{
    public class Bundle
    {
        public AssetBundle Asset { get; private set; }
        public string Hash { get; private set; }

        public Bundle(AssetBundle asset)
        {
            Asset = asset;
            Hash = string.Empty;
        }

        public Bundle(AssetBundle asset, string hash)
        {
            Asset = asset;
            Hash = hash;
        }

        public override string ToString()
        {
            return $"{Asset.name}, hash: {Hash}";
        }
    }
}
