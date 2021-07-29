using BundlesLoader.Gif;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(GifImage))]
    public class GifBundleLoader : BundleLoader
    {
        private GifImage gifImage;

        protected override void Awake()
        {
            base.Awake();
            gifImage = GetComponent<GifImage>();
            Initialize();
        }

        public void Initialize()
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

                var gifAsset = asset.LoadAsset<TextAsset>(split[2]);
                if (gifAsset == null)
                {
                    Debug.LogError($"No asset in bundle with name:{split[2]}");
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