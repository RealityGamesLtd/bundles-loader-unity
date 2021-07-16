using UnityEngine;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class TextureBundleLoader : BundleLoader
    {
        public abstract void SetSprite(Sprite sprite);

        protected override void Awake()
        {
            base.Awake();
            if (Application.isEditor)
            {
                var split = bundleType.FullName.Split('/');
                if (split.Length != 3)
                {
                    return;
                }

                var sprite = AssetLoader.GetAsset<Sprite>(
                    split[1],
                    split[2]);

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
                    Debug.LogError($"No asset bundle with name:{split[1]}");
                    return;
                }

                var texture = asset.LoadAsset<Texture2D>(split[2]);
                if (texture == null)
                {
                    Debug.LogError($"No asset in bundle with name:{split[2]}");
                    return;
                }

                SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}


