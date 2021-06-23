using BundlesLoader.Bundles.Core;
using BundlesLoader.Service;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class BundleLoader : MonoBehaviour
    {
        [SerializeField] protected BundleType bundleType = new BundleType();

        public BundleType BundleType { get => bundleType; }

        protected IAssetsService assetsService;

        protected virtual void Awake()
        {
            assetsService = AssetsServiceLoader.AssetsService;
        }
    }
}


