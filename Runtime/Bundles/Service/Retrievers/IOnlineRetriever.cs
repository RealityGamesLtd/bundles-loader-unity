using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
using BundlesLoader.Callbacks;
using System.IO;
using BundlesLoader.Bundles.Core;
using Newtonsoft.Json;

namespace BundlesLoader.Service.Retrievers
{
    public class NoCertHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public class IOnlineRetriever : Retriever, IBundleRetriever
    {
        private const int CACHE_COUNT_MAX = 1;
        private const string BUNDLES_SUBDIRECTORY = "Bundles";
        private const int TIMEOUT_TIME_SECONDS = 10;

        public Action<float> ProgressCallback { get; private set; }
        public Action<IEntityCallback> BundleLoadedCallback { get; set; }

        private readonly string AssetBundleVersionDirectory;

        public IOnlineRetriever(Dictionary<string, BundleVersion> versions, string assetBundleVersionDirectory,
            string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            AssetBundleVersionDirectory = assetBundleVersionDirectory;
            Versions = versions;
            ProgressCallback = progressCallback;
        }

        public async Task GetBundle(string name, CancellationToken ct, Action<string, Bundle> func)
        {
            await WaitForCache();

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("ONLINE PROVIDER: No versions!");
                return;
            }

            if (Versions.TryGetValue(name, out var bund))
            {
                var res = await RetrieveBundle(name, bund.Hash, ct);
                if (res.Item2 != null)
                {
                    func?.Invoke(res.Item1, res.Item2);
                }
            }
        }

        public async Task GetBundles(CancellationToken ct, Action<string, Bundle> func)
        {
            if (string.IsNullOrEmpty(AssetBundleVersionDirectory))
            {
                Debug.LogError($"ONLINE PROVIDER: No directory for current game version: {Application.version}!");
                return;
            }

            await WaitForCache();

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("ONLINE PROVIDER: No versions!");
                return;
            }

            var tasks = Versions.Select((x) =>
            {
                return GetBundle(x.Key, ct, func);
            }).Where(x => x != null).ToArray();

            int count = 0;

            if (tasks == null)
            {
                Debug.LogError("ONLINE PROVIDER: Tasks for  bundles are null!");
                return;
            }

            foreach (var task in tasks)
            {
                await task;
                count++;
                ProgressCallback?.Invoke((float)count / Versions.Count);
            }

            SaveOnlineVersions();
        }

        private void SaveOnlineVersions()
        {
            var path = Path.Combine(Application.temporaryCachePath, BUNDLES_SUBDIRECTORY);
            if (Directory.Exists(path))
            {
                var fileName = $"{path}/version.json";
                if (File.Exists(fileName)) File.Delete(fileName);
                if (Versions != null)
                {
                    var json = JsonConvert.SerializeObject(Versions);
                    File.WriteAllText(fileName, json);
                }
            }
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, string hash, CancellationToken ct)
        {
            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{AssetBundleVersionDirectory}/" +
                $"{name}";

            var versions = new List<Hash128>();
            Caching.GetCachedVersions(name, versions);
            var parsedHash = Hash128.Parse(hash);

            if (Caching.IsVersionCached(name, parsedHash))
            {
                Debug.LogWarning($"ONLINE PROVIDER: Bundle: {name}, with this hash: {hash} is already cached");

                if (IsBundleLoadedWithSameCache(name))
                {
                    Debug.LogWarning($"ONLINE PROVIDER: Bundle: {name}, with this hash: {hash} is already loaded, omitting!");
                    return new Tuple<string, Bundle>(name, null);
                }
            }
            else
            {
                Debug.LogWarning($"ONLINE PROVIDER: Bundle {name} no cached version founded for this hash: {hash}...");
            }

            using var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, parsedHash);
            uwr.timeout = TIMEOUT_TIME_SECONDS;
            uwr.certificateHandler = new NoCertHandler();
            uwr.disposeCertificateHandlerOnDispose = true;
            uwr.disposeDownloadHandlerOnDispose = true;
            uwr.SendWebRequest();

            while (!uwr.isDone && !ct.IsCancellationRequested)
                await Task.Yield();

            if (ct.IsCancellationRequested)
            {
                Debug.LogError($"ONLINE PROVIDER: Bundle {name} getting failed due canceled task!");
                BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE, BundleErrorType.FAILED,
                    $"Bundle {name} getting failed due canceled task!", name));
                uwr.Abort();
                return new Tuple<string, Bundle>(name, null);
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"ONLINE PROVIDER: Bundle {name} getting failed due to error: {uwr.error}!");
                BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE, BundleErrorType.FAILED,
                    $"Bundle {name} getting failed due to error: {uwr.error}!", name));
                uwr.Dispose();
                return new Tuple<string, Bundle>(name, null);
            }

            if (!CheckForDirectory(name, parsedHash))
            {
                var handlerError = !string.IsNullOrEmpty(uwr.downloadHandler.error) ? $"Message: {uwr.downloadHandler.error}" : string.Empty;
                Debug.LogError($"ONLINE PROVIDER: Failed to get bundle: {name}, directory to read from doesn't exist!" + handlerError);
                BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE,
                    BundleErrorType.FAILED, $"Bundle: {name} - failed to get bundle content, directory to read from doesn't exist!", name));
                uwr.Dispose();
                return new Tuple<string, Bundle>(name, null);
            }

            UnloadCurrentBundle(name);
            RemoveCurrentBundleFromCache(name, parsedHash);

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle == null)
            {
                Debug.LogError($"ONLINE PROVIDER: Failed to get bundle: {name}, due to bundle is null!" +
                     $"Message: {uwr.downloadHandler.error}");
                BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE, BundleErrorType.NULL_BUNDLE, $"{name} no bundle downloaded!", name));
                uwr.Dispose();
                return new Tuple<string, Bundle>(name, null);
            }
            else
            {
                LogRequestResponseStatus(name, uwr);
                uwr.Dispose();

                var assets = bundle.GetAllAssetNames();
                if (assets == null || assets.Length == 0)
                {
                    Debug.LogError($"ONLINE PROVIDER: Bundle: {name}, is empty!");
                    BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE, BundleErrorType.EMPTY_BUNDLE, $"{name} bundle is empty!", name));
                    return new Tuple<string, Bundle>(name, null);
                }

                var res = await LoadAssets(bundle);
                return new Tuple<string, Bundle>(name, new Bundle(res, name, hash));
            }
        }

        private bool IsBundleLoadedWithSameCache(string name)
        {
            var bundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            if (bundles != null && bundles.Count > 0)
            {
                var loadedBundle = bundles.Find(x => x.name.Equals(name));
                if (loadedBundle != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void UnloadCurrentBundle(string name)
        {
            var bundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            if(bundles != null && bundles.Count > 0)
            {
                var loadedBundle = bundles.Find(x => x.name.Equals(name));
                if (loadedBundle != null)
                {
                    loadedBundle.Unload(true);
                }
            }
        }

        private bool CheckForDirectory(string name, Hash128 parsedHash)
        {
            var path = Path.Combine(Path.Combine(Caching.currentCacheForWriting.path, name), parsedHash.ToString());
            if (!Directory.Exists(path))
            {
                return false;
            }
            return true;
        }

        private void RemoveCurrentBundleFromCache(string name, Hash128 parsedHash)
        {
            var versions = new List<Hash128>();
            Caching.GetCachedVersions(name, versions);
            if (versions.Count > CACHE_COUNT_MAX)
            {
                var versionToDelete = versions.FindAll(x => !x.Equals(parsedHash));
                if (versionToDelete != null)
                {
                    var flags = versionToDelete.Select(x => Caching.ClearCachedVersion(name, x)).ToList();
                    if (flags.Contains(false))
                        Debug.LogError($"ONLINE PROVIDER: Clearing: {name} failed!");
                }
            }

            versions.Clear();
            Caching.GetCachedVersions(name, versions);
            if (versions.Count == CACHE_COUNT_MAX)
            {
                var first = versions.First();
                if (first != null)
                {
                    if (!first.Equals(parsedHash))
                        Debug.LogError($"ONLINE PROVIDER: Clearing: {name} wrong hash was cached!");
                }
            }
        }

        private void LogRequestResponseStatus(string name, UnityWebRequest uwr)
        {
            if (string.IsNullOrEmpty(name) || uwr == null)
            {
                Debug.LogError("ONLINE PROVIDER: Name or web request is null!");
                return;
            }

            var responseHeaders = uwr.GetResponseHeaders();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                if (responseHeaders == null)
                {
                    Debug.Log($"ONLINE PROVIDER: Bundle {name} loaded from cache!");
                }
                else
                {
                    Debug.Log($"ONLINE PROVIDER: Bundle {name} version was downloaded from server");
                }
            }
        }
    }
}
