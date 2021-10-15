using BundlesLoader.Bundles.Core;
using UnityEngine;
using System;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class RuntimeTextureBundleLoader : BundleLoader
    {
        protected abstract void SetSprite(Sprite sprite);

        [Obsolete("Use LoadSprite(AssetType) instead")]
        public void LoadSprite(string bundleName, string spriteName)
        {
            var assetsService = AssetsServiceLoader.AssetsService;

            var sprite = AssetRetriever.GetSprite(assetsService, bundleName, spriteName);
            if (sprite != null) SetSprite(sprite);
        }

        [Obsolete("Use LoadSprite(AssetType) instead")]
        public void LoadSprite(string bundleName, string spriteName, string atlasName)
        {
            var assetsService = AssetsServiceLoader.AssetsService;

            var sprite = AssetRetriever.GetSprite(assetsService, bundleName, spriteName, atlasName);
            if (sprite != null) SetSprite(sprite);
        }

        public void LoadSprite(AssetType assetType)
        {
            var sprite = AssetRetriever.GetSprite(assetType);
            if (sprite != null) SetSprite(sprite);
        }
    }
}


