using BundlesLoader.Callbacks;
using BundlesLoader.Gif;
using BundlesLoader.Service;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    [RequireComponent(typeof(GifSprite))]
    public class GifSpriteBundleLoader : BundleLoader, IRefreshable
    {
        private GifSprite gifSprite;

        protected void Awake()
        {
            gifSprite = GetComponent<GifSprite>();
            Initialize();
        }

        public void Initialize()
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
                bundle.OnAssetsChanged += OnAssetsChanged;
                SetCurrentAsset(bundle, false);
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleType.BundleName}");
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

        public void SetCurrentAsset(Bundle bundle, bool forceSwap)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return;
            }

            if (string.IsNullOrEmpty(bundleType.EntityName))
            {
                Debug.LogError($"Empty entity asset name: {bundleType.EntityName}");
                return;
            }

            var gifAsset = bundle.LoadAsset<TextAsset>(bundleType.EntityName);
            if (gifAsset == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no gif:{bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_GIF, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no gif:{bundleType.EntityName}",
                    $"{bundleType.RootName}/{bundleType.BundleName}", bundleType.EntityName));
                return;
            }

            gifSprite.Initialize(GifConverter.LoadGifSprite(bundleType.EntityName, gifAsset.bytes, forceSwap));
        }

        public void OnAssetsChanged(Bundle currentBundle)
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            SetCurrentAsset(currentBundle, true);
        }
    }
}