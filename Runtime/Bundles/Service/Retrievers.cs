using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace BundlesLoader.Service
{
    public class IOfflineRetriever : IBundleRetriever
    {
        private const string STREAMING_ASSETS_PATH = "Assets/StreamingAssets/Bundles";
        private readonly string ASSET_BUNDLES_URL;

        private Action<float> callback;

        public IOfflineRetriever(string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            callback = progressCallback;
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
            int count = 0;

            var bundlesTask = files.Select(x =>
            {
                return RetrieveBundle(x, ct);
            }).Where(x => x != null).ToArray();

            if (bundlesTask == null)
            {
                Debug.LogError("Tasks for  bundles are null!");
                return datas;
            }

            foreach (var task in bundlesTask)
            {
                var res = await task;
                count++;
                callback?.Invoke((float)count / files.Length);
                if(res.Item2 != null)
                {
                    if (!datas.ContainsKey(res.Item1))
                    {
                        datas.Add(res.Item1, res.Item2);
                    }
                    else
                    {
                        Debug.Log($"Dictionary already with this key: {res.Item1}");
                    }
                }
                else
                {
                    Debug.LogError($"{res.Item1} no bundle downloaded!");
                }
            }

            return datas;
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, CancellationToken ct)
        {
            Tuple<string, Bundle> loadedBundle;

            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{Path.GetFileName(name)}";
            List<Hash128> listOfCachedVersions = new List<Hash128>();
            Caching.GetCachedVersions(Path.GetFileName(name), listOfCachedVersions);

            if (listOfCachedVersions.Count < 1)
            {
                var fileTask = AssetBundle.LoadFromFileAsync(Path.Combine(STREAMING_ASSETS_PATH, Path.GetFileName(name)));
                while (!fileTask.isDone)
                    await Task.Yield();

                loadedBundle = new Tuple<string, Bundle>(Path.GetFileName(name), new Bundle(fileTask.assetBundle, string.Empty));
            }
            else
            {
                var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, listOfCachedVersions.Last());
                uwr.SendWebRequest();

                while (!uwr.isDone)
                    await Task.Yield();

                if (ct.IsCancellationRequested || uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"Bundle {Path.GetFileName(name)} loading canceled due to error!");
                    return new Tuple<string, Bundle>(Path.GetFileName(name), null);
                }

                var bund = DownloadHandlerAssetBundle.GetContent(uwr);
                loadedBundle = new Tuple<string, Bundle>(Path.GetFileName(name), new Bundle(bund, listOfCachedVersions.Last().ToString()));
            }
            return loadedBundle;
        }
    }

    public class IOnlineRetriever : IBundleRetriever
    {
        private const int CACHE_COUNT_MAX = 5;
        private readonly string ASSET_BUNDLES_URL;
        private readonly Dictionary<string, string> Versions;

        private Action<float> callback;

        public IOnlineRetriever(Dictionary<string, string> versions, string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = versions;
            callback = progressCallback;
        }

        public async Task<Dictionary<string, Bundle>> GetBundles
            (CancellationToken ct)
        {
            Dictionary<string, Bundle> datas = new Dictionary<string, Bundle>();
            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("No versions!");
                return datas;
            }

            var tasks = Versions.Select((x) =>
            {
                return RetrieveBundle(x.Key, x.Value, ct);
            }).Where(x => x != null).ToArray();

            int count = 0;

            if (tasks == null)
            {
                Debug.LogError("Tasks for  bundles are null!");
                return datas;
            }

            foreach (var task in tasks)
            {
                var res = await task;
                count++;
                callback?.Invoke((float)count / Versions.Count);

                if(res.Item2 != null)
                {
                    if (!datas.ContainsKey(res.Item1))
                    {
                        datas.Add(res.Item1, res.Item2);
                    }
                    else
                    {
                        Debug.Log($"Dictionary already with this key: {res.Item1}");
                    }
                }
                else
                {
                    Debug.LogError($"{res.Item1} no bundle downloaded!");
                }

            }
            return datas;
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, string hash, CancellationToken ct)
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

            if (ct.IsCancellationRequested)
            {
                Debug.LogError($"Bundle {name} loading canceled due to error!");
                return new Tuple<string, Bundle>(name, null);
            }

            using var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Parse(hash));
            uwr.SendWebRequest();

            while (!uwr.isDone)
                await Task.Yield();

            if (ct.IsCancellationRequested || uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError($"Bundle {name} loading canceled due to error!");
                return new Tuple<string, Bundle>(name, null);
            }

            UpdateCachedVersions(name);
            LogRequestResponseStatus(name, uwr);
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle == null)
            {
                Debug.LogError($"Failed to get bundle content!");
                return new Tuple<string, Bundle>(name, null);
            }
            else
            {
                return new Tuple<string, Bundle>(name, new Bundle(bundle, hash));
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
            if (string.IsNullOrEmpty(name) || uwr == null)
            {
                Debug.LogError("Name or web request is null!");
                return;
            }

            var responseHeaders = uwr.GetResponseHeaders();
            if (uwr.result == UnityWebRequest.Result.Success)
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
