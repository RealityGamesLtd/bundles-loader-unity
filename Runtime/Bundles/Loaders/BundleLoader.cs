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
    }
}


