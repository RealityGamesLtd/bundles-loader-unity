using BundlesLoader.Gif;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(GifSprite))]
    public class GifSpriteBundleLoader : BundleLoader
    {
        private GifSprite gifSprite;

        protected void Awake()
        {
            gifSprite = GetComponent<GifSprite>();
            Initialize();
        }

        public void Initialize()
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
                    Debug.LogError($"No asset bundle with name:{split[1]}");
                    return;
                }

                var gifAsset = asset.LoadAsset<TextAsset>(split[2]);
                if (gifAsset == null)
                {
                    Debug.LogError($"No asset in bundle with name:{split[2]}");
                    return;
                }

                gifSprite.Load(gifAsset.bytes);
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}