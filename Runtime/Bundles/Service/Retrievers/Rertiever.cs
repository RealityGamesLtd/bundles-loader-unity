using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BundlesLoader.Service.Retrievers
{
    public abstract class Retriever
    {
        protected string ASSET_BUNDLES_URL;
        protected Dictionary<string, string> Versions;

        protected async Task WaitForCache()
        {
            while (!Caching.ready)
            {
                await Task.Yield();
            }
        }

        protected async Task<Object[]> LoadAssets(AssetBundle bundle)
        {
            var req = bundle.LoadAllAssetsAsync();
            while (req.isDone)
                await Task.Yield();

            var assets = req.allAssets;
            return assets;
        }
    }
}
