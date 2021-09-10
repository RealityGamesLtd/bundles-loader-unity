using BundlesLoader.Bundles.Core;
using UnityEngine;
using System;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class RuntimeTextureBundleLoader : BundleLoader
    {
        protected abstract void SetSprite(Sprite sprite);

        [Obsolete("Use LoadSprite(AssetType) instead")]
        public void LoadSprite(string bundleName, string assetName, string spriteInAtlas = "")
        {
            var assetsService = AssetsServiceLoader.AssetsService;

            var sprite = AssetRetriever.GetSprite(assetsService, bundleName, assetName, spriteInAtlas);
            if (sprite != null) SetSprite(sprite);
        }

        public void LoadSprite(AssetType assetType)
        {
            var sprite = AssetRetriever.GetSprite(assetType);
            if (sprite != null) SetSprite(sprite);
        }
    }
}


