using BundlesLoader.Bundles.Core;
using System;
using UnityEngine;
using UnityEngine.U2D;

namespace BundlesLoader
{
    public static class AssetRetriever
    {
        public static Sprite GetSprite(AssetType assetType)
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService == null) return null;

            var pathComponents = assetType.GetPathComponents();

            switch (pathComponents)
            {
                case SpriteAtlasAssetPathComponents spriteAtlasAsset:
                    return GetSprite(assetsService, spriteAtlasAsset.BundleName, spriteAtlasAsset.AssetName, spriteAtlasAsset.SpriteAtlasName);
                case AssetPathComponents asset:
                    return GetSprite(assetsService, asset.BundleName, asset.AssetName);
                default:
                    Debug.LogError($"Unsupported path components type");
                    return null;
            }
        }

        [Obsolete("Use GetSprite(AssetType) instead")]
        public static Sprite GetSprite(Service.IAssetsService assetsService, string bundleName, string assetName, string spriteInAtlas = "")
        {
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

            var asset = bundle.Asset;
            if (asset == null)
            {
                Debug.LogError($"No asset bundle with name: {bundleName}");
                return null;
            }

            Sprite sprite;
            if (spriteInAtlas != string.Empty)
            {
                var atlas = asset.LoadAsset<SpriteAtlas>(assetName);
                if (atlas == null)
                {
                    Debug.LogError($"No asset in bundle with name: {assetName}");
                    return null;
                }

                sprite = atlas.GetSprite(spriteInAtlas);
                if (sprite == null)
                {
                    Debug.LogError($"No sprite in atlas with name: {spriteInAtlas}");
                    return null;
                }
            }
            else
            {
                sprite = asset.LoadAsset<Sprite>(assetName);
                if (sprite == null)
                {
                    Debug.LogError($"No sprite in bundle with name: {assetName}");
                    return null;
                }
            }

            return sprite;
        }
    }
}
