using UnityEngine;
using UnityEngine.U2D;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class SpriteAtlasBundleLoader : BundleLoader
    {
        public abstract void SetSprite(Sprite sprite);

        protected override void Awake()
        {
            base.Awake();
            if (Application.isEditor)
            {
                var split = bundleType.FullName.Split('/');
                if (split.Length != 4)
                {
                    return;
                }

                var spriteAtlas = AssetLoader.GetAsset<SpriteAtlas>(
                    split[1],
                    split[2]);

                if (spriteAtlas == null)
                {
                    Debug.LogError($"No sprite atlas found: {bundleType.FullName}");
                    return;
                }

                var sprite = spriteAtlas.GetSprite(split[3]);

                if (sprite == null)
                {
                    Debug.LogError($"No sprite to show: {bundleType.FullName}");
                    return;
                }

                SetSprite(sprite);
            }
            else Initialize();
        }

        private void Initialize()
        {
            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!");
                return;
            }

            var split = bundleType.FullName.Split('/');
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
                    Debug.LogError($"No asset bundle with name:{split[1]}");
                    return;
                }

                var atlas = asset.LoadAsset<SpriteAtlas>(split[2]);
                if (atlas == null)
                {
                    Debug.LogError($"No asset in bundle with name:{split[2]}");
                    return;
                }

                var sprite = atlas.GetSprite(split[3]);
                if (sprite == null)
                {
                    Debug.LogError($"No sprite in atlas with name: {split[3]}");
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