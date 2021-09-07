using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Bundles.Utils
{
    public class AssetBundlesChecker
    {
        private const string ASSET_BUNDLE_PATH = "Assets/AssetBundles";

        public static RootObject GetBundlesNames()
        {
            RootObject root = new RootObject();

            if (!Directory.Exists(ASSET_BUNDLE_PATH))
            {
                Debug.LogError("No directory found!");
                return root;
            }

            if (!File.Exists($"{ASSET_BUNDLE_PATH}/names.json"))
            {
                Debug.LogError("No bundles names file for config!");
                return root;
            }

            var content = File.ReadAllText($"{ASSET_BUNDLE_PATH}/names.json");
            var names = JsonConvert.DeserializeObject<RootObject>(content, new RootObjectConverter());
            return names;
        }
    }
}


