using BundlesLoader.Service;

namespace BundlesLoader.Bundles.Loaders
{
    public interface IRefreshable
    {
        public void OnAssetsChanged(Bundle currentBundle);
    }
}


