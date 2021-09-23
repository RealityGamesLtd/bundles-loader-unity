using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BundlesLoader.Callbacks;

namespace BundlesLoader.Service
{
    public interface IAssetsService
    {
        Dictionary<string, Bundle> Bundles { get; }
        Task<Dictionary<string, Bundle>> GetAssets(CancellationToken ct);
        void LogErrorAsset(IEntityCallback message);

        Action<float> OnProgressChanged { get; set; }
        Action<IEntityCallback> OnErrorCallback { get; set; }
    }
}
