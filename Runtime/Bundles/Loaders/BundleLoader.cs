using BundlesLoader.Bundles.Core;
using BundlesLoader.Callbacks;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class BundleLoader : MonoBehaviour
    {
        [SerializeField] protected BundleType bundleType = new BundleType();

        public BundleType BundleType { get => bundleType; }

        protected RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void LogError(IEntityCallback callback) => AssetsServiceLoader.AssetsService.LogErrorAsset(callback);

        protected bool IsValidAssetsService()
        {
            var assetsService = AssetsServiceLoader.AssetsService;
            if (assetsService == null)
            {
                Debug.LogError("Asset Service is not loaded!", gameObject);
                return false;
            }

            if (assetsService.Bundles == null)
            {
                Debug.LogError("Asset Bundles not loaded!", gameObject);
                return false;
            }

            if (bundleType == null || string.IsNullOrEmpty(bundleType.FullName))
            {
                Debug.LogError("Bundle type not loaded!", gameObject);
                return false;
            }

            return true;
        }

        public void OnValidate()
        {
            if (string.IsNullOrEmpty(bundleType.FullName))
            {
                return;
            }

            var split = bundleType.FullName.Split('/');
            if (split.Length >= 3)
            {
                bundleType.BundleName = split[1];
                bundleType.RootName = split[0];
            }

            if (split.Length == 4)
                bundleType.EntityName = split[3];
            else if (split.Length == 3)
                bundleType.EntityName = split[2];
        }
    }
}


