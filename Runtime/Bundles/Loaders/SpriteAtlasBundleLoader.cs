using BundlesLoader.Callbacks;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class SpriteAtlasBundleLoader : BundleLoader
    {
        public abstract void SetSprite(Sprite sprite);

        protected void Awake()
        {
            if (Application.isEditor)
            {
                if (bundleType == null || bundleType.FullName == null)
                {
                    Debug.LogError("Bundle type not loaded!");
                    return;
                }

                var split = bundleType.FullName?.Split('/');
                if (split.Length != 4)
                {
                    Debug.LogError($"Wrong format: {bundleType.FullName} !");
                    return;
                }

                var spriteAtlas = AssetLoader.GetAsset<SpriteAtlas>(
                    split[1],
                    split[2]);

                if (spriteAtlas == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}");
                    return;
                }

                var sprite = spriteAtlas.GetSprite(split[3]);

                if (sprite == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}");
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
            if (split.Length != 4)
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

                var atlas = asset.LoadAsset<SpriteAtlas>(split[2]);
                if (atlas == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}");
                    LogError(new AssetCallback(AssetErrorType.NULL_SPRITEATLAS, $"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}",
                        $"{split[0]}/{split[1]}", split[2]));
                    return;
                }

                var sprite = atlas.GetSprite(split[3]);
                if (sprite == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}");
                    LogError(new AssetCallback(AssetErrorType.NULL_SPRITE, $"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}",
                        $"{split[0]}/{split[1]}/{split[2]}", split[3]));
                    return;
                }

                SetSprite(sprite);
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}