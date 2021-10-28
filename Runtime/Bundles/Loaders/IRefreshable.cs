using BundlesLoader.Service;

namespace BundlesLoader.Bundles.Loaders
{
    public interface IRefreshable
    {
        public void SetCurrentAsset(string[] split, Bundle bundle);
        public void OnAssetsChanged(Bundle currentBundle);
    }
}


