using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BundlesLoader.Service
{
    public interface IAssetsService
    {
        Dictionary<string, Bundle> Bundles { get; }
        Task<Dictionary<string, Bundle>> GetAssets(CancellationToken ct);
    }
}
