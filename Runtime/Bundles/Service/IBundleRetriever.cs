using System;
using System.Threading;
using System.Threading.Tasks;
using BundlesLoader.Callbacks;

namespace BundlesLoader.Service
{
    public interface IBundleRetriever
    {
        Task GetBundles(CancellationToken ct, Action<string, Bundle> func);
        Task GetBundle(string name, CancellationToken ct, Action<string, Bundle> func);
        Action<IEntityCallback> BundleLoadedCallback { get; set; }
        Action<float> ProgressCallback { get; }
    }
}