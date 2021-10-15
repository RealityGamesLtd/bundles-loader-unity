using BundlesLoader.Callbacks;
using BundlesLoader.Gif;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(GifImage))]
    public class GifBundleLoader : BundleLoader
    {
        private GifImage gifImage;

        protected void Awake()
        {
            gifImage = GetComponent<GifImage>();
            Initialize();
        }

        public void Initialize()
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if(assetsService == null)
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
                    LogError(new BundleCallback(RetrieverType.LOADER, BundleErrorType.NO_BUNDLE, $"No specified asset bundle:{split[1]}", $"{split[0]}/{split[1]}"));
                    return;
                }

                var gifAsset = asset.LoadAsset<TextAsset>(split[2]);
                if (gifAsset == null)
                {
                    Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no gif:{split[2]}");
                    LogError(new AssetCallback(AssetErrorType.NULL_GIF, $"Bundle:{split[0]}/{split[1]} -> no gif:{split[2]}",
                        $"{split[0]}/{split[1]}", split[2]));
                    return;
                }

                gifImage.Load(gifAsset.bytes);
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }
    }
}