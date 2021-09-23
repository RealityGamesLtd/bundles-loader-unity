using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BundlesLoader.Callbacks;

namespace BundlesLoader.Service
{
    public interface IBundleRetriever
    {
        Task<Dictionary<string, Bundle>> GetBundles(CancellationToken ct);
        Action<IEntityCallback> BundleLoadedCallback { get; set; }
        Action<float> ProgressCallback { get; }
    }
}