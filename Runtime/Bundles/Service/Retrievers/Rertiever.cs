using System.Collections.Generic;
using System.Threading.Tasks;
using BundlesLoader.Bundles.Core;
using UnityEngine;

namespace BundlesLoader.Service.Retrievers
{
    public abstract class Retriever
    {
        protected string ASSET_BUNDLES_URL;
        protected Dictionary<string, BundleVersion> Versions;

        protected async Task WaitForCache()
        {
            while (!Caching.ready)
            {
                await Task.Yield();
            }
        }
    }
}
