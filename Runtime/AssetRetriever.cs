using BundlesLoader.Service;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

namespace BundlesLoader
{
    public static class AssetRetriever
    {
        public static Bundle GetBundle(string bundleName)
        {
            var assetsService = AssetsServiceLoader.AssetsService;

            if (assetsService == null)
            {
                Debug.LogError("Asset Service is not loaded!");
                return null;
            }

            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!");
                return null;
            }

            if (assetsService.Bundles.TryGetValue(bundleName, out var bundle) == false)
            {
                Debug.LogError($"No bundle found with name: {bundleName}");
                return null;
            }

            return bundle;
        }

        public static bool IsAsset(string name)
        {
            var bundles = AssetsServiceLoader.AssetsService.Bundles;
            bool ret = false;

            if (string.IsNullOrEmpty(name))
                return ret;

            foreach (var bundle in bundles)
            {
                var assets = bundle.Value.Assets;
                if (assets != null && assets.Count > 0)
                {
                    var obj = assets.First().Value;
                    if (obj is SpriteAtlas atl)
                        ret = CheckForSpriteInAtlas(name, atl);
                    else
                        ret = CheckForTexture(name, assets);
                }

                if (ret)
                    break;
            }

            return ret;
        }

        private static bool CheckForTexture(string name, Dictionary<string, Object> assets)
        {
            bool ret = false;

            if(assets.TryGetValue(name, out var asset))
            {
                if (asset != null) ret = true;
            }

            return ret;
        }

        private static bool CheckForSpriteInAtlas(string name, SpriteAtlas atl)
        {
            bool ret = false;
            var sprite = Path.GetFileNameWithoutExtension(name);

            if (!string.IsNullOrEmpty(sprite) && atl.GetSprite(sprite) != null)
                ret = true;
            return ret;
        }
    }
}
