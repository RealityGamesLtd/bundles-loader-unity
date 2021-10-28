using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using BundlesLoader.Service;
using BundlesLoader.Service.Retrievers;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;

namespace Tests
{
    public class MemoryBundleTests : MonoBehaviour
    {
        private const string BUNDLES_SUBDIRECTORY = "Bundles";
        private const string BUNDLE_NAME = "assets_bg";
        private const string ASSET_BUNDLES_URL = "https://assets.wearerealitygames.com/landlord-go/bundles";
        private const int COUNTER = 25;

        private Dictionary<string, Bundle> cache;
        private string TESTING_OUTPUT_PATH;

        private void Awake()
        {
            TESTING_OUTPUT_PATH = $"{Application.dataPath}/Tests";
            cache = new Dictionary<string, Bundle>();
            SetUpDirectory();
            SetCurrentCache();
        }

        private void SetUpDirectory()
        {
            if (!Directory.Exists(TESTING_OUTPUT_PATH))
                Directory.CreateDirectory(TESTING_OUTPUT_PATH);
        }

        private void Start()
        {
            StartCoroutine(DownloadAndLoadBundleMultiple());
        }

        public IEnumerator DownloadAndLoadBundleMultiple()
        {
            Debug.Log($"STARTING OFFLINE");

            yield return GetOffline();

            Debug.Log($"STARTING ONLINE ITERATIONS");

            for (int i = 0; i < COUNTER; ++i)
            {
                Debug.Log($"ITERATION:{i}");

                var versions = GetVersionsOnline();
                var currentRetriever = new IOnlineRetriever(versions, ASSET_BUNDLES_URL, (float t) => { });
                var task = currentRetriever.GetBundle(BUNDLE_NAME, default, LoadBundle);

                while (!task.IsCompleted)
                {
                    yield return null;
                }

                MemoryProfiler.TakeSnapshot($"{TESTING_OUTPUT_PATH}/snapshot_online_{i}.snap", (string t, bool fl) => { });
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator GetOffline()
        {
            var versions = GetVersionsOffline();
            var currentRetriever = new IOfflineRetriever(versions, ASSET_BUNDLES_URL, (float t) => { });
            var task = currentRetriever.GetBundle(BUNDLE_NAME, default, LoadBundle);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            MemoryProfiler.TakeSnapshot($"{TESTING_OUTPUT_PATH}/snapshot_offline.snap", (string t, bool fl) => { });
            yield return new WaitForSeconds(1f);
        }

        private void SetCurrentCache()
        {
            Cache currCache;
            var path = Path.Combine(Application.persistentDataPath, BUNDLES_SUBDIRECTORY);
            if (Directory.Exists(path))
                currCache = Caching.AddCache(path);
            else
            {
                Directory.CreateDirectory(path);
                currCache = Caching.AddCache(path);
            }

            List<string> cachePaths = new List<string>();
            Caching.GetAllCachePaths(cachePaths);

            if (currCache.valid)
            {
                Caching.currentCacheForWriting = currCache;
            }

            Debug.Log(Caching.currentCacheForWriting.path);
        }

        private void LoadBundle(string name, Bundle bundle)
        {
            if (cache.ContainsKey(name))
            {
                var currentHash = cache[name].Hash;
                var nextHash = bundle.Hash;
                if (!nextHash.Equals(currentHash))
                {
                    if (bundle.Assets != null)
                        cache[name].Update(bundle.Assets, bundle.Hash);
                    else
                        Debug.LogError($"New bundle: {name}, has null asset value, omitting puting into dictionary!");
                }
            }
            else
            {
                cache.Add(name, bundle);
            }
        }

        private Dictionary<string, string> GetVersionsOnline()
        {
            var dict = new Dictionary<string, string>();

            var bytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            dict.Add(BUNDLE_NAME, BitConverter.ToString(bytes).Replace("-", "").ToLower());
            return dict;
        }

        private Dictionary<string, string> GetVersionsOffline()
        {
            var dict = new Dictionary<string, string>();
            dict.Add(BUNDLE_NAME, "e72ae61c307db8ec0a67051c0c31ea36");
            return dict;
        }
    }
}
