using BundlesLoader.Callbacks;
using BundlesLoader.Gif;
using BundlesLoader.Service;
using UnityEngine;
using Utils;

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
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{assetName}";

            if (!IsValidAssetsService())
            {
                return;
            }

            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService.Bundles.TryGetValue(bundleName, out var bundle))
            {
                bundle.OnAssetsChanged += OnAssetsChanged;
                SetCurrentAsset(bundleType.FullName.Split('/'), bundle);

            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleName}");
            }
        }

        private void OnDestroy()
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            var assetsService = AssetsServiceLoader.AssetsService;

            var split = bundleType.FullName?.Split('/');
            if (split.Length != 3)
            {
                Debug.LogError($"Wrong format: {bundleType.FullName} !");
                return;
            }

            if (assetsService.Bundles.TryGetValue(split[1], out var bundle))
            {
                bundle.OnAssetsChanged -= OnAssetsChanged;
            }
            else
            {
                Debug.LogError($"No bundle with name: {split[1]}");
            }
        }

        public void SetCurrentAsset(string[] split, Bundle bundle)
        {
            var gifAsset = bundle.LoadAsset<TextAsset>(split[2]);
            if (gifAsset == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no gif:{split[2]}");
                LogError(new AssetCallback(AssetErrorType.NULL_GIF, $"Bundle:{split[0]}/{split[1]} -> no gif:{split[2]}",
                    $"{split[0]}/{split[1]}", split[2]));
                return;
            }

            gifImage.Load(gifAsset.bytes);
        }

        public void OnAssetsChanged(Bundle currentBundle)
        {
            var split = bundleType.FullName?.Split('/');
            if (split.Length != 3)
            {
                Debug.LogError($"Wrong format: {bundleType.FullName} !");
                return;
            }

            SetCurrentAsset(split, currentBundle);
        }
    }
}