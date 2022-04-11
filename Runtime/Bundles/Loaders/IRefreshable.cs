using BundlesLoader.Service;

namespace BundlesLoader.Bundles.Loaders
{
    public interface IRefreshable
    {
        public void SetCurrentAsset(Bundle bundle);
        public void OnAssetsChanged(Bundle currentBundle);
    }
}


