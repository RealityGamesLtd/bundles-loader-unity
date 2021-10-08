using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Bundles.Utils
{
    public class AssetBundlesChecker
    {
        public static RootObject GetBundlesNames()
        {
            RootObject root = new RootObject();

            if (!Directory.Exists(Symbols.ASSET_BUNDLE_PATH))
            {
                Debug.LogError("No directory found!");
                return root;
            }

            if (!File.Exists($"{Symbols.ASSET_BUNDLE_PATH}/names.json"))
            {
                Debug.LogError("No bundles names file for config!");
                return root;
            }

            var content = File.ReadAllText($"{Symbols.ASSET_BUNDLE_PATH}/names.json");

            RootObject names;
            try
            {
                names = JsonConvert.DeserializeObject<RootObject>(content, new RootObjectConverter());
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return root;
            }

            return names;
        }
    }
}


