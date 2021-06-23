using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public class AssetBundlesPreprocessor : IPreprocessBuildWithReport
    {
        private const string ASSET_BUNDLE_PATH = "Assets/AssetBundles";
        private const string STREAMING_ASSETS_PATH = "Assets/StreamingAssets/Bundles";

        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            var bundles = Directory.GetFiles($"{ASSET_BUNDLE_PATH}/{EditorUserBuildSettings.activeBuildTarget}")
                .Where(x => !Path.GetFileName(x).Equals(EditorUserBuildSettings.activeBuildTarget.ToString())
                            && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            if (!Directory.Exists($"{STREAMING_ASSETS_PATH}"))
            {
                Debug.LogError("Streaming Asset Bundle directory doesn't exist! Creating directory!");
            }
            else
            {
                var streamingFiles = Directory.GetFiles($"{STREAMING_ASSETS_PATH}").ToArray();
                for (int i = 0; i < streamingFiles.Length; ++i)
                {
                    File.Delete(streamingFiles[i]);
                }
            }
            for (int i = 0; i < bundles.Length; ++i)
            {
                var newPath = $"{STREAMING_ASSETS_PATH}/{Path.GetFileName(bundles[i])}";
                File.Copy(bundles[i], newPath);
                AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}