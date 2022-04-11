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

        public bool Initialize(string bundleName, string assetName)
        {
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{assetName}";
            bundleType.RootName = Symbols.BUNDLES_SUBDIRECTORY;
            bundleType.BundleName = bundleName;
            bundleType.EntityName = assetName;

            if (!IsValidAssetsService())
            {
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return false;
            }

            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService.Bundles.TryGetValue(bundleType.BundleName, out var bundle))
            {
                bundle.OnAssetsChanged += OnAssetsChanged;
                return SetCurrentAsset(bundle);
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleType.BundleName}");
                return false;
            }
        }

        private void OnDestroy()
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            var assetsService = AssetsServiceLoader.AssetsService;
            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return;
            }

            if (assetsService.Bundles.TryGetValue(bundleType.BundleName, out var bundle))
            {
                bundle.OnAssetsChanged -= OnAssetsChanged;
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleType.BundleName}");
            }
        }

        public bool SetCurrentAsset(Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.EntityName))
            {
                Debug.LogError($"No entity name set up: {bundleType.FullName}!");
                return false;
            }

            var gifAsset = bundle.LoadAsset<TextAsset>(bundleType.EntityName);
            if (gifAsset == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no gif:{bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_GIF, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no gif:{bundleType.EntityName}",
                    $"{bundleType.RootName}/{bundleType.BundleName}", bundleType.EntityName));
                return false;
            }

            gifImage.Load(gifAsset.bytes);
            return true;
        }

        public void OnAssetsChanged(Bundle currentBundle)
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            SetCurrentAsset(currentBundle);
        }
    }
}