using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BundlesLoader.Service
{
    public class IOfflineRetriever : IBundleRetriever
    {
        private const string STREAMING_ASSETS_PATH = "Assets/StreamingAssets/Bundles";
        private readonly string ASSET_BUNDLES_URL;

        public IOfflineRetriever(string assetBundlesUrl)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
        }

        public async Task<Dictionary<string, Bundle>> GetBundles
            (CancellationToken ct)
        {
            Dictionary<string, Bundle> datas = new Dictionary<string, Bundle>();

            if (!Directory.Exists(STREAMING_ASSETS_PATH))
            {
                Debug.LogError("No directory found!");
                return datas;
            }

            var files = Directory.GetFiles(STREAMING_ASSETS_PATH).Where(x => string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();    
            var bundlesTask = files.Select(async x =>
            {
                return await RetrieveBundle(x);
            });

            var res = await Task.WhenAll(bundlesTask);
            for(int i = 0; i < res.Length; ++i)
            {           
                datas.Add(res[i].Asset.name, res[i]);
            }

            return datas;
        }

        private async Task<Bundle> RetrieveBundle(string name)
        {
            Bundle loadedBundle;

            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{Path.GetFileName(name)}";
            List<Hash128> listOfCachedVersions = new List<Hash128>();
            Caching.GetCachedVersions(Path.GetFileName(name), listOfCachedVersions);

            if(listOfCachedVersions.Count < 1)
            {
                var fileTask = AssetBundle.LoadFromFileAsync(Path.Combine(STREAMING_ASSETS_PATH, Path.GetFileName(name)));
                while (!fileTask.isDone)
                    await Task.Yield();

                loadedBundle = new Bundle(fileTask.assetBundle, string.Empty);
            }
            else
            {
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, listOfCachedVersions.Last());
                var req = request.SendWebRequest();
                while (!req.isDone)
                    await Task.Yield();

                var bund = DownloadHandlerAssetBundle.GetContent(request);
                loadedBundle = new Bundle(bund, listOfCachedVersions.Last().ToString());
            }

            return loadedBundle;
        }
    }

    public class IOnlineRetriever : IBundleRetriever
    {
        private const int CACHE_COUNT_MAX = 5;
        private readonly string ASSET_BUNDLES_URL;
        private readonly Dictionary<string, string> Versions;

        public IOnlineRetriever(Dictionary<string, string> versions, string assetBundlesUrl)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = versions;
        }

        public async Task<Dictionary<string, Bundle>> GetBundles
            (CancellationToken ct)
        {
            Dictionary<string, Bundle> datas = new Dictionary<string, Bundle>();
            var tasks = Versions.Select(async (x) =>
            {
                return (x.Key, await RetrieveBundle(x.Key, x.Value));
            });

            (string name, Bundle data)[] result = await Task.WhenAll(tasks);
            foreach (var (name, data) in result)
            {
                if (data != null)
                {
                    datas.Add(name, data);
                }
            }
            return datas;
        }

        private async Task<Bundle> RetrieveBundle(string name, string hash)
        {
            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{name}";

            var versions = new List<Hash128>();
            Caching.GetCachedVersions(name, versions);

            if (Caching.IsVersionCached(url, Hash128.Parse(hash)))
            {
                Debug.Log($"Bundle {name} with this hash is already cached!");
            }
            else
            {
                Debug.Log($"Bundle {name} no cached version founded for this hash...");
            }

            using var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Parse(hash));
            uwr.SendWebRequest();

            while (!uwr.isDone)
                await Task.Yield();

            UpdateCachedVersions(name);
            LogRequestResponseStatus(name, uwr);

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle == null)
            {
                Debug.LogError($"Failed to get bundle content!");
                return null;
            }
            else
            {
                return new Bundle(bundle, hash);
            }
        }

        private void UpdateCachedVersions(string name)
        {
            var versions = new List<Hash128>();
            Caching.GetCachedVersions(name, versions);

            if (versions.Count > CACHE_COUNT_MAX)
                Caching.ClearCachedVersion(name, versions.First());
        }

        private void LogRequestResponseStatus(string name, UnityWebRequest uwr)
        {
            if(string.IsNullOrEmpty(name) || uwr == null)
            {
                Debug.LogError("Name or web request is null!");
                return;
            }

            var responseHeaders = uwr.GetResponseHeaders();
            if(uwr.result == UnityWebRequest.Result.Success)
            {
                if (responseHeaders == null)
                {
                    Debug.Log($"Bundle {name} loaded from cache!");
                }
                else
                {
                    Debug.Log($"Bundle {name} version was downloaded from server");
                }
            }
            else
            {
                Debug.Log($"Bundle {name} not downloaded, with error {uwr.error}");
            }
        }
    }
}
