using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BundlesLoader.EditorHelpers.Tools.Bundles.Parsers;
using UnityEditor;
using UnityEngine;
using Utils;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public static class AssetBundleBuilder
    {
        public static bool BuildBundles(string currentTarget, List<Container> objects, bool freshBuild, string minVersion, string maxVersion)
        {
            if (objects.Count <= 0)
            {
                Debug.LogError("No objects to save!");
                return true;
            }

            if (freshBuild)
            {
                if (Directory.Exists($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}"))
                {
                    var filesToDelete = Directory.GetFiles($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}").ToArray();
                    for (int i = 0; i < filesToDelete.Length; ++i)
                    {
                        File.Delete(filesToDelete[i]);
                    }
                }
            }

            for(int i = 0; i < objects.Count; ++i)
            {
                var name = objects[i].BundleName;
                objects[i].SelectedObjects.ForEach(z => AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(z)).SetAssetBundleNameAndVariant(name, ""));
            }

            AssetBundleBuild[] buildMap = new AssetBundleBuild[objects.Count];
            for (int i = 0; i < buildMap.Length; ++i)
            {
                buildMap[i].assetNames = objects[i].SelectedObjects.Select(x => AssetDatabase.GetAssetPath(x)).ToArray();
                buildMap[i].assetBundleName = objects[i].BundleName;
            }

            if (!Directory.Exists($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}"))
            {
                Debug.LogError("Asset Bundle directory doesn't exist! Creating directory!");
                Directory.CreateDirectory($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}");
            }

            BuildTarget target = System.Enum.TryParse<BuildTarget>(currentTarget, out var res) ?
                res : EditorUserBuildSettings.activeBuildTarget;
            var manifest = BuildPipeline.BuildAssetBundles($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}", buildMap,
                BuildAssetBundleOptions.None, target);

            var files = Directory.GetFiles($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}");
            for (int i = 0; i < files.Length; ++i)
            {
                AssetDatabase.ImportAsset(files[i], ImportAssetOptions.ForceUpdate);
            }

            GenerateVersions(currentTarget, objects, minVersion, maxVersion);
            SetUpEditorAssets(currentTarget);

            return GenerateNames(objects);
        }

        private static void SetUpEditorAssets(string currentTarget)
        {
            var bundles = Directory.GetFiles($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}")
                .Where(x => !Path.GetFileName(x).Equals(currentTarget) && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            if (!Directory.Exists($"{Symbols.STREAMING_ASSET_BUNDLE_PATH}"))
            {
                Debug.LogError("Streaming Asset Bundle directory doesn't exist! Creating directory!");
                Directory.CreateDirectory($"{Path.Combine(Application.streamingAssetsPath, Symbols.BUNDLES_SUBDIRECTORY)}");
            }
            else
            {
                var streamingFiles = Directory.GetFiles($"{Symbols.STREAMING_ASSET_BUNDLE_PATH}").ToArray();
                for (int i = 0; i < streamingFiles.Length; ++i)
                {
                    File.Delete(streamingFiles[i]);
                }
            }
            for (int i = 0; i < bundles.Length; ++i)
            {
                var newPath = $"{Symbols.STREAMING_ASSET_BUNDLE_PATH}/{Path.GetFileName(bundles[i])}";
                File.Copy(bundles[i], newPath);
                AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            }
        }

        private static bool GenerateNames(List<Container> objects)
        {
            string currentNames = string.Empty;

            if (File.Exists($"{Symbols.ASSET_BUNDLE_PATH}/{Symbols.NAMES_FILE_NAME}"))
            {
                try
                {
                    currentNames = File.ReadAllText($"{Symbols.ASSET_BUNDLE_PATH}/{Symbols.NAMES_FILE_NAME}");
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    return false;
                }
            }

            var json = AssetBundlesNamesCreator.CreateNames(objects);
            File.WriteAllText($"{Symbols.ASSET_BUNDLE_PATH}/{Symbols.NAMES_FILE_NAME}", json);
            AssetDatabase.ImportAsset($"{Symbols.ASSET_BUNDLE_PATH}/{Symbols.NAMES_FILE_NAME}", ImportAssetOptions.ForceUpdate);

            return currentNames.Equals(json);
        }

        private static void GenerateVersions(string currentTarget, List<Container> objects, string  minVersion, string maxVersion)
        {
            var files = Directory.GetFiles($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}")
                .Where(x => !Path.GetFileName(x).Equals(currentTarget) && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            var json = AssetBundlesVersionsCreator.CreateVersions(files, objects, minVersion, maxVersion);
            File.WriteAllText($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}/{Symbols.VERSION_FILE_NAME}", json);
            AssetDatabase.ImportAsset($"{Symbols.ASSET_BUNDLE_PATH}/{currentTarget}/{Symbols.VERSION_FILE_NAME}", ImportAssetOptions.ForceUpdate);
        }
    }
}
