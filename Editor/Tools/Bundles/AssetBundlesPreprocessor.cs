using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Utils;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public class AssetBundlesPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            var assetBundlesPath = $"{Symbols.ASSET_BUNDLE_PATH}/{EditorUserBuildSettings.activeBuildTarget}";

            if (Directory.Exists(assetBundlesPath))
            {
                var files = Directory.GetFiles(assetBundlesPath).ToList();

                var bundles = files.Where(x => !Path.GetFileName(x).Equals(EditorUserBuildSettings.activeBuildTarget.ToString())
                    && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();
                var versionFile = files.Find(x => Path.GetExtension(x).Equals(".json"));

                if(bundles != null && versionFile !=  null)
                {
                    var path = Path.Combine(Application.streamingAssetsPath, Symbols.BUNDLES_SUBDIRECTORY);

                    if (!Directory.Exists(path))
                    {
                        Debug.LogWarning("Streaming Asset Bundle directory doesn't exist! Creating directory!");
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        var streamingFiles = Directory.GetFiles(path).ToArray();
                        for (int i = 0; i < streamingFiles.Length; ++i)
                        {
                            File.Delete(streamingFiles[i]);
                        }
                    }

                    for (int i = 0; i < bundles.Length; ++i)
                    {
                        var bundlePath = Path.Combine(path, Path.GetFileName(bundles[i]));
                        File.Copy(bundles[i], bundlePath);
                        AssetDatabase.ImportAsset(bundlePath, ImportAssetOptions.ForceUpdate);
                    }

                    if (versionFile != null)
                    {
                        var versionPath = Path.Combine(path, Path.GetFileName(versionFile));
                        File.Copy(versionFile, versionPath);
                        AssetDatabase.ImportAsset(versionPath, ImportAssetOptions.ForceUpdate);
                    }
                    else
                        Debug.LogError("No version file found!");
                }
                else
                    Debug.LogError($"Bundles or version file not found inside: {assetBundlesPath}!");
            }
            else
                Debug.LogError($"No directory {assetBundlesPath}!");
        }
    }
}