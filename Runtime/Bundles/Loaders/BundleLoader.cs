using BundlesLoader.Bundles.Core;
using BundlesLoader.Callbacks;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class BundleLoader : MonoBehaviour
    {
        [SerializeField] protected BundleType bundleType = new BundleType();

        public BundleType BundleType { get => bundleType; }

        public void LogError(IEntityCallback callback) => AssetsServiceLoader.AssetsService.LogErrorAsset(callback);

        protected bool IsValidAssetsService()
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService == null)
            {
                Debug.LogError("Asset Service is not loaded!");
                return false;
            }

            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!");
                return false;
            }

            if (bundleType == null || string.IsNullOrEmpty(bundleType.FullName))
            {
                Debug.LogError("Bundle type not loaded!");
                return false;
            }

            return true;
        }
    }
}


