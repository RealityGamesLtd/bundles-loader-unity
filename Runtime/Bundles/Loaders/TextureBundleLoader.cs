using BundlesLoader.Callbacks;
using UnityEngine;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class TextureBundleLoader : BundleLoader
    {
        public abstract void SetSprite(Sprite sprite);

        protected void Awake()
        {
            if (Application.isEditor)
            {
                if(bundleType == null || bundleType.FullName == null)
                {
                    Debug.LogError("Bundle type not loaded!");
                    return;
                }

                var split = bundleType.FullName.Split('/');
                if (split.Length != 3)
                {
                    Debug.LogError($"Wrong format: {bundleType.FullName} !");
                    return;
                }

                var sprite = AssetLoader.GetAsset<Sprite>(
                    split[1],
                    split[2]);

                if (sprite == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}");
                    return;
                }

                SetSprite(sprite);
            }
            else Initialize();
        }

        private void Initialize()
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService == null)
            {
                Debug.LogError("Asset Service is not loaded!");
                return;
            }

            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!");
                return;
            }

            if (bundleType == null || bundleType.FullName == null)
            {
                Debug.LogError("Bundle type not loaded!");
                return;
            }

            var split = bundleType.FullName?.Split('/');
            if (split.Length != 3)
            {
                Debug.LogError($"Wrong format: {bundleType.FullName} !");
                return;
            }

            if (assetsService.Bundles.TryGetValue(split[1], out var bundle))
            {
                var asset = bundle.Asset;
                if (asset == null)
                {
                    Debug.LogError($"No specified asset bundle:{split[1]}");
                    LogError(new BundleCallback(BundleErrorType.NO_BUNDLE, $"No specified asset bundle:{split[1]}", $"{split[0]}/{split[1]}"));
                    return;
                }

                var texture = asset.LoadAsset<Texture2D>(split[2]);
                var sprite = asset.LoadAsset<Sprite>(split[2]);
                if (texture == null && sprite == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}");
                    LogError(new AssetCallback(AssetErrorType.NULL_TEXTURE, $"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}",
                        $"{split[0]}/{split[1]}", split[2]));
                    return;
                }

                if (texture != null)
                    SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
                else if (sprite != null)
                    SetSprite(sprite);
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}


