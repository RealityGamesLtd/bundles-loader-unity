using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(Image))]
    public class RuntimeTextureBundleLoader : BundleLoader
    {
        private Image image;

        protected override void Awake()
        {
            base.Awake();
            image = GetComponent<Image>();
        }

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
                    sprite = asset.LoadAsset<Sprite>(spriteInAtlas);
                    if (sprite == null)
                    {
                        Debug.LogError($"No sprite in bundle with name: {assetName}");
                        return;
                    }
                }


                image.sprite = sprite;
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleName}");
            }
        }
    }
}

