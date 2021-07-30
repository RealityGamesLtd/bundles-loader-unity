using System;
using UnityEngine;
using UnityEngine.U2D;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class RuntimeTextureBundleLoader : BundleLoader
    {
        protected abstract void SetSprite(Sprite sprite);

        public void LoadSprite(string bundleName, string assetName, string spriteInAtlas = "")
        {
            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!");
                return;
            }

            if (assetsService.Bundles.TryGetValue(bundleName, out var bundle))
            {
                var asset = bundle.Asset;
                if (asset == null)
                {
                    Debug.LogError($"No asset bundle with name:{bundleName}");
                    return;
                }

                Sprite sprite = null;

                if (spriteInAtlas != string.Empty)
                {
                    var atlas = asset.LoadAsset<SpriteAtlas>(assetName);
                    if (atlas == null)
                    {
                        Debug.LogError($"No asset in bundle with name:{assetName}");
                        return;
                    }

                    sprite = atlas.GetSprite(spriteInAtlas);
                    if (sprite == null)
                    {
                        Debug.LogError($"No sprite in atlas with name: {spriteInAtlas}");
                        return;
                    }
                }
                else
                {
                    sprite = asset.LoadAsset<Sprite>(assetName);
                    if (sprite == null)
                    {
                        Debug.LogError($"No sprite in bundle with name: {assetName}");
                        return;
                    }
                }

                SetSprite(sprite);
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleName}");
            }
        }
    }
}


