using BundlesLoader.Bundles.Core;
using BundlesLoader.Service;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

namespace BundlesLoader
{
    public static class AssetRetriever
    {
        public static Bundle GetBundle(AssetType assetType)
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService == null) return null;

            var pathComponents = assetType.GetPathComponents();

            switch (pathComponents)
            {
                case SpriteAtlasAssetPathComponents spriteAtlasAsset:
                    return GetBundle(spriteAtlasAsset.BundleName);
                case AssetPathComponents asset:
                    return GetBundle(asset.BundleName);
                default:
                    Debug.LogError($"Unsupported path components type");
                    return null;
            }
        }

        [Obsolete("Use GetSprite(AssetType) instead")]
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

            foreach(var bundle in bundles)
            {
                var assets = bundle.Value.Assets;
                if(assets != null)
                {
                    var obj = assets.First();
                    if(obj is SpriteAtlas atl)
                    {
                        var sprite = Path.GetFileNameWithoutExtension(name);
                        ret = !string.IsNullOrEmpty(sprite) && atl.GetSprite(sprite) != null;
                    }
                    else
                    {
                        var asset = assets.Find(x => x.name.Equals(name));
                        if (asset != null)
                            ret = true;
                    }
                }
            }

            return ret;
        }
    }
}
