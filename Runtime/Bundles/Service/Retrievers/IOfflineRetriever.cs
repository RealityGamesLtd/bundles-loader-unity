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
using BundlesLoader.Bundles.Core;

namespace BundlesLoader.Service.Retrievers
{
    public class IOfflineRetriever : Retriever, IBundleRetriever
    {
        public Action<float> ProgressCallback { get; private set; }
        public Action<IEntityCallback> BundleLoadedCallback { get; set; }

        protected Dictionary<string, BundleVersion> CachedVersions;

        public IOfflineRetriever(Dictionary<string, BundleVersion> streamingVersions, Dictionary<string, BundleVersion> chachedVersions,
            string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = streamingVersions;
            CachedVersions = chachedVersions;
            ProgressCallback = progressCallback;
        }

        public async Task GetBundle(string name, CancellationToken ct, Action<string, Bundle> func)
        {
            await WaitForCache();

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("OFFLINE PROVIDER: No versions!");
                return;
            }

            if (Versions.TryGetValue(name, out var hash))
            {
                var res = await RetrieveBundle(name, hash.Hash, ct);
                if (res.Item2 != null)
                {
                    func?.Invoke(res.Item1, res.Item2);
                }
            }
        }

        public async Task GetBundles(CancellationToken ct, Action<string, Bundle> func)
        {
            await WaitForCache();

            if (Versions == null || Versions.Count == 0)
            {
                Debug.LogError("OFFLINE PROVIDER: No versions!");
                return;
            }

            var files = Versions.Select(x => x.Key).ToArray();
            int count = 0;

            var bundlesTask = files.Select(x =>
            {
                return GetBundle(x, ct, func);
            }).Where(x => x != null).ToArray();

            if (bundlesTask == null)
            {
                Debug.LogError("OFFLINE PROVIDER: Tasks for  bundles are null!");
                return;
            }

            foreach (var task in bundlesTask)
            {
                await task;
                count++;
                ProgressCallback?.Invoke((float)count / files.Length);
            }
        }

        private async Task<Tuple<string, Bundle>> RetrieveBundle(string name, string hash, CancellationToken ct)
        {
            Tuple<string, Bundle> loadedBundle;

            var url = $"{ASSET_BUNDLES_URL}/" +
                $"{PlatformDictionary.GetDirectoryByPlatform(Application.platform)}/" +
                $"{name}";
            List<Hash128> listOfCachedVersions = new List<Hash128>();
            Caching.GetCachedVersions(name, listOfCachedVersions);

            var currentVersion = Application.version.ToString();
            var useCached = CheckForVersionToDownload(name, currentVersion, listOfCachedVersions);

            //If no cached bundles are present and we are offline (First game run)
            if (!useCached)
            {
                AssetBundleCreateRequest fileTask = null;
                try
                {
                    fileTask = AssetBundle.LoadFromFileAsync(
                        Path.Combine(Path.Combine(Application.streamingAssetsPath, Symbols.BUNDLES_SUBDIRECTORY), name));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                if (fileTask == null)
                {
                    Debug.LogError($"Gettting file task from streaming assets is null: {name}!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                }
                else
                {
                    while (!fileTask.isDone)
                        await Task.Yield();

                    var bundle = fileTask.assetBundle;

                    if (bundle != null)
                    {
                        var res = await LoadAssets(bundle);
                        loadedBundle = new Tuple<string, Bundle>(name, new Bundle(res, name, hash));
                    }
                    else
                    {
                        Debug.LogError($"OFFLINE PROVIDER: Failed to get bundle: {name} content from streaming assets!");
                        loadedBundle = new Tuple<string, Bundle>(name, null);
                    }
                }
            }
            //If there are some cached bundles present and we want to get them instead of local bundles from files (Second game run)
            else
            {
                var parsedHash = listOfCachedVersions.Last();
                var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, listOfCachedVersions.Last());
                uwr.SendWebRequest();

                while (!uwr.isDone)
                    await Task.Yield();

                if (ct.IsCancellationRequested || uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"OFFLINE PROVIDER: Bundle {name} loading canceled due to error: {uwr.error}!");
                    BundleLoadedCallback?.Invoke(
                        new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.FAILED, $"Bundle {name} getting error:{uwr.error}!", name));
                    return new Tuple<string, Bundle>(name, null);
                }

                if (!CheckForDirectory(name, parsedHash))
                {
                    var handlerError = !string.IsNullOrEmpty(uwr.downloadHandler.error) ? $"Message: {uwr.downloadHandler.error}" : string.Empty;
                    Debug.LogError($"OFFLINE PROVIDER: Failed to get bundle: {name}, directory to read from doesn't exist!" + handlerError);
                    BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.ONLINE,
                        BundleErrorType.FAILED, $"Bundle: {name} - failed to get bundle content, directory to read from doesn't exist!", name));
                    return new Tuple<string, Bundle>(name, null);
                }

                UnloadCurrentBundle(name);

                var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bundle == null)
                {
                    Debug.LogError($"OFFLINE PROVIDER: Failed to get bundle: {name} content due to content is null!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                    BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.NULL_BUNDLE, $"{name} no bundle downloaded!", name));
                }
                else
                {
                    var assets = bundle.GetAllAssetNames();
                    if (assets == null || assets.Length == 0)
                    {
                        Debug.LogError($"OFFLINE PROVIDER: Bundle: {name}, is empty!");
                        BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.EMPTY_BUNDLE, $"{name} bundle is empty!", name));
                        return new Tuple<string, Bundle>(name, null);
                    }

                    var res = await LoadAssets(bundle);
                    loadedBundle = new Tuple<string, Bundle>(name, new Bundle(res, name, listOfCachedVersions.ToString()));
                }
            }
            return loadedBundle;
        }

        private bool CheckForVersionToDownload(string name, string currentVersion, List<Hash128> listOfCachedVersions)
        {
            bool downloadCachedVersion = false;

            if (CachedVersions.TryGetValue(name, out var cachedVersion) &&
                Versions.TryGetValue(name, out var streamedVersion))
            {
                if (streamedVersion.MinVersion.Equals(currentVersion))
                    downloadCachedVersion = false;
                else if (cachedVersion.MaxVersion.Equals(currentVersion))
                    downloadCachedVersion = listOfCachedVersions.Count > 0;
            }

            return downloadCachedVersion;
        }

        private void UnloadCurrentBundle(string name)
        {
            var bundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
            if (bundles != null && bundles.Count > 0)
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
    }
}
