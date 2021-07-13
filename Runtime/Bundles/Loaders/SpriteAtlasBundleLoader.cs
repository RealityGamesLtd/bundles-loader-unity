using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class SpriteAtlasBundleLoader : BundleLoader
    {
        public abstract void SetSprite(Sprite sprite);

        protected override void Awake()
        {
            base.Awake();
            Initialize();
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

                SetSprite(sprite);;
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}