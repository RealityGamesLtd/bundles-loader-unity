using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
using BundlesLoader.Callbacks;
using Utils;

namespace BundlesLoader.Service
{
    public class IOfflineRetriever : IBundleRetriever
    {
        private readonly string ASSET_BUNDLES_URL;
        private readonly Dictionary<string, string> Versions;

        public Action<float> ProgressCallback { get; private set; }
        public Action<IEntityCallback> BundleLoadedCallback { get; set; }

        public IOfflineRetriever(Dictionary<string, string> versions, string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = versions;
            ProgressCallback = progressCallback;
        }

        public async Task GetBundles(CancellationToken ct, Action<string, Bundle> func)
        {
            while (!Caching.ready)
            {
                await Task.Yield();
            }

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("No versions!");
                return;
            }

            var files = Versions.Select(x => x.Key).ToArray();
            int count = 0;

            var bundlesTask = files.Select(x =>
            {
                return RetrieveBundle(x, ct);
            }).Where(x => x != null).ToArray();

            if (bundlesTask == null)
            {
                Debug.LogError("Tasks for  bundles are null!");
                return;
            }

            foreach (var task in bundlesTask)
            {
                var res = await task;
                count++;

                if (res.Item2 != null)
                {
                    func?.Invoke(res.Item1, res.Item2);
                }
                ProgressCallback?.Invoke((float)count / files.Length);
            }
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, CancellationToken ct)
        {
            Tuple<string, Bundle> loadedBundle;

            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{name}";
            List<Hash128> listOfCachedVersions = new List<Hash128>();
            Caching.GetCachedVersions(name, listOfCachedVersions);

            //If no cached bundles are present and we are offline (First game run)
            if (listOfCachedVersions.Count < 1)
            {
                var fileTask = AssetBundle.LoadFromFileAsync(
                    Path.Combine(Path.Combine(Application.streamingAssetsPath, Symbols.BUNDLES_SUBDIRECTORY), name));
                while (!fileTask.isDone)
                    await Task.Yield();

                if (fileTask.assetBundle != null)
                {
                    loadedBundle = new Tuple<string, Bundle>(name, new Bundle(fileTask.assetBundle, string.Empty));
                }
                else
                {
                    Debug.LogError($"Failed to get bundle content!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                }
            }
            //If there are some cached bundles present and we want to get them instead of local bundles from files (Second game run)
            else
            {
                var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, listOfCachedVersions.Last());
                uwr.SendWebRequest();

                while (!uwr.isDone)
                    await Task.Yield();

                if (ct.IsCancellationRequested || uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Bundle {name} loading canceled due to error: {uwr.error}!");
                    BundleLoadedCallback?.Invoke(
                        new BundleCallback(BundleErrorType.FAILED, $"Bundle {name} getting error:{uwr.error}!", name));
                    return new Tuple<string, Bundle>(name, null);
                }

                var bund = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bund == null)
                {
                    Debug.LogError($"Failed to get bundle content!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                    BundleLoadedCallback?.Invoke(new BundleCallback(BundleErrorType.NULL_BUNDLE, $"{name} no bundle downloaded!", name));
                }
                else
                {
                    var assets = bund.GetAllAssetNames();
                    if (assets == null || assets.Length == 0)
                    {
                        BundleLoadedCallback?.Invoke(new BundleCallback(BundleErrorType.EMPTY_BUNDLE, $"{name} bundle is empty!", name));
                    }
                    loadedBundle = new Tuple<string, Bundle>(name, new Bundle(bund, listOfCachedVersions.Last().ToString()));
                }
            }
            return loadedBundle;
        }
    }

    public class IOnlineRetriever : IBundleRetriever
    {
        private const int CACHE_COUNT_MAX = 2;
        private readonly string ASSET_BUNDLES_URL;
        private readonly Dictionary<string, string> Versions;

        public Action<float> ProgressCallback { get; private set; }
        public Action<IEntityCallback> BundleLoadedCallback { get; set; }

        public IOnlineRetriever(Dictionary<string, string> versions, string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = versions;
            ProgressCallback = progressCallback;
        }

        public async Task GetBundles(CancellationToken ct, Action<string, Bundle> func)
        {
            while (!Caching.ready)
            {
                await Task.Yield();
            }

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("No versions!");
                return;
            }

            var tasks = Versions.Select((x) =>
            {
                return RetrieveBundle(x.Key, x.Value, ct);
            }).Where(x => x != null).ToArray();

            int count = 0;

            if (tasks == null)
            {
                Debug.LogError("Tasks for  bundles are null!");
                return;
            }

            foreach (var task in tasks)
            {
                var res = await task;
                count++;

                if (res.Item2 != null)
                {
                    func?.Invoke(res.Item1, res.Item2);
                }
                ProgressCallback?.Invoke((float)count / Versions.Count);
            }
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, string hash, CancellationToken ct)
        {
            //Handler for new bundles from server

            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{name}";

            var versions = new List<Hash128>();
            Caching.GetCachedVersions(name, versions);
            var parsedHash = Hash128.Parse(hash);

            if (Caching.IsVersionCached(name, parsedHash) && IsBundleLoaded(name))
            {
                Debug.Log($"Bundle {name} with this hash is already cached/downloaded and loaded into memory, omitting...!");
                return new Tuple<string, Bundle>(name, null);
            }
            else
            {
                Debug.Log($"Bundle {name} no cached version founded for this hash...");
            }

            using var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, parsedHash);
            uwr.SendWebRequest();

            while (!uwr.isDone)
                await Task.Yield();

            if (ct.IsCancellationRequested || uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Bundle {name} getting failed due to error: {uwr.error}!");
                BundleLoadedCallback?.Invoke(
                    new BundleCallback(BundleErrorType.FAILED, $"Bundle {name} getting failed due to error: {uwr.error}!", name));
                return new Tuple<string, Bundle>(name, null);
            }

            UpdateCachedVersions(name);
            LogRequestResponseStatus(name, uwr);
            UnloadRedundantAsset(name);

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle == null)
            {
                Debug.LogError($"Failed to get bundle content!");
                BundleLoadedCallback?.Invoke(new BundleCallback(BundleErrorType.NULL_BUNDLE, $"{name} no bundle downloaded!", name));
                return new Tuple<string, Bundle>(name, null);
            }
            else
            {
                var assets = bundle.GetAllAssetNames();
                if (assets == null || assets.Length == 0)
                {
                    BundleLoadedCallback?.Invoke(new BundleCallback(BundleErrorType.EMPTY_BUNDLE, $"{name} bundle is empty!", name));
                }
                return new Tuple<string, Bundle>(name, new Bundle(bundle, hash));
            }
        }

        private bool IsBundleLoaded(string name)
        {
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            if (loadedBundles != null)
            {
                var list = loadedBundles.ToList();
                var loadedBundle = list.Find(x => x.name.Equals(name));
                if (loadedBundle != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void UnloadRedundantAsset(string name)
        {
            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            if (loadedBundles != null)
            {
                var list = loadedBundles.ToList();
                var loadedBundle = list.Find(x => x.name.Equals(name));
                if (loadedBundle != null)
                {
                    loadedBundle.Unload(false);
                }
                else
                {
                    Debug.LogWarning($"No bundle:{name} loaded!");
                }
            }
            else
            {
                Debug.LogWarning("No bundles loaded!");
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
        }
    }
}
