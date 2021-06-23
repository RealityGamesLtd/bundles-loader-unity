using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BundlesLoader.Service
{
    public interface IBundleRetriever
    {
        Task<Dictionary<string, Bundle>> GetBundles(CancellationToken ct);
    }
}