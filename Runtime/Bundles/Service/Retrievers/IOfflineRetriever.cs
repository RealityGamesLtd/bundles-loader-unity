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

namespace BundlesLoader.Service.Retrievers
{
    public class IOfflineRetriever : Retriever, IBundleRetriever
    {
        public Action<float> ProgressCallback { get; private set; }
        public Action<IEntityCallback> BundleLoadedCallback { get; set; }

        public IOfflineRetriever(Dictionary<string, string> versions, string assetBundlesUrl, Action<float> progressCallback)
        {
            ASSET_BUNDLES_URL = assetBundlesUrl;
            Versions = versions;
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

            if (Versions.TryGetValue(name, out var bund))
            {
                var res = await RetrieveBundle(name, ct);
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
                AssetBundleCreateRequest fileTask = null;
                try
                {
                    fileTask = AssetBundle.LoadFromFileAsync(
                        Path.Combine(Path.Combine(Application.streamingAssetsPath, Symbols.BUNDLES_SUBDIRECTORY), name));
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                }

                if(fileTask == null)
                {
                    Debug.LogError($"Gettting file task from streaming assets is null: {name}!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                }
                else
                {
                    while (!fileTask.isDone)
                        await Task.Yield();

                    if (fileTask.assetBundle != null)
                    {
                        Debug.Log($"OFFLINE PROVIDER: {fileTask.assetBundle.name} was loaded from streaming assets!");
                        loadedBundle = new Tuple<string, Bundle>(name, new Bundle(fileTask.assetBundle, string.Empty));
                    }
                    else
                    {
                        Debug.LogError($"OFFLINE PROVIDER: Failed to get bundle content from streaming assets!");
                        loadedBundle = new Tuple<string, Bundle>(name, null);
                    }
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
                    Debug.LogError($"OFFLINE PROVIDER: Bundle {name} loading canceled due to error: {uwr.error}!");
                    BundleLoadedCallback?.Invoke(
                        new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.FAILED, $"Bundle {name} getting error:{uwr.error}!", name));
                    return new Tuple<string, Bundle>(name, null);
                }

                var bund = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bund == null)
                {
                    Debug.LogError($"OFFLINE PROVIDER: Failed to get bundle content!");
                    loadedBundle = new Tuple<string, Bundle>(name, null);
                    BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.NULL_BUNDLE, $"{name} no bundle downloaded!", name));
                }
                else
                {
                    var assets = bund.GetAllAssetNames();
                    if (assets == null || assets.Length == 0)
                    {
                        BundleLoadedCallback?.Invoke(new BundleCallback(RetrieverType.OFFLINE, BundleErrorType.EMPTY_BUNDLE, $"{name} bundle is empty!", name));
                    }

                    Debug.LogError($"OFFLINE PROVIDER: {name} bundle loaded from cache succesfully!");
                    loadedBundle = new Tuple<string, Bundle>(name, new Bundle(bund, listOfCachedVersions.Last().ToString()));
                }
            }
            return loadedBundle;
        }
    }
}
