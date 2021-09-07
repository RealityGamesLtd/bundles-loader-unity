using BundlesLoader.Gif;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(GifImage))]
    public class RuntimeGifImageBundleLoader : BundleLoader
    {
        private GifImage gifImage;

        protected void Awake()
        {
            gifImage = GetComponent<GifImage>();
        }

        public void Initialize(string bundleName, string assetName)
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

            if (assetsService.Bundles.TryGetValue(bundleName, out var bundle))
            {
                var asset = bundle.Asset;
                if (asset == null)
                {
                    Debug.LogError($"No asset bundle with name:{bundleName}");
                    return;
                }

                var gifAsset = asset.LoadAsset<TextAsset>(assetName);
                if (gifAsset == null)
                {
                    Debug.LogError($"No asset in bundle with name:{assetName}");
                    return;
                }

                gifImage.Load(gifAsset.bytes);
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleName}");
            }
        }
    }
}