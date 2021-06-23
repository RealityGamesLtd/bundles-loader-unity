using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public static class AssetBundleBuilder
    {
        private const string ASSET_BUNDLE_PATH = "Assets/AssetBundles";
        private const string STREAMING_ASSETS_PATH = "Assets/StreamingAssets/Bundles";

        public static void BuildBundles(string currentTarget)
        {
            if (!Directory.Exists($"{ASSET_BUNDLE_PATH}/{currentTarget}"))
            {
                Debug.LogError("Asset Bundle directory doesn't exist! Creating directory!");
                Directory.CreateDirectory($"{ASSET_BUNDLE_PATH}/{currentTarget}");
            }

            BuildTarget target = System.Enum.TryParse<BuildTarget>(currentTarget, out var res) ?
                res : EditorUserBuildSettings.activeBuildTarget;
            BuildPipeline.BuildAssetBundles($"{ASSET_BUNDLE_PATH}/{currentTarget}",
                BuildAssetBundleOptions.None, target);

            GenerateVersions(currentTarget);
            SetUpEditorAssets(currentTarget);
        }

        public static bool BuildBundles(string currentTarget, List<Container> objects, bool freshBuild)
        {
            if (objects.Count <= 0)
            {
                Debug.LogError("No objects to save!");
                return true;
            }

            if (freshBuild)
            {
                if (Directory.Exists($"{ASSET_BUNDLE_PATH}/{currentTarget}"))
                {
                    var filesToDelete = Directory.GetFiles($"{ASSET_BUNDLE_PATH}/{currentTarget}").ToArray();
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

            if (!Directory.Exists($"{ASSET_BUNDLE_PATH}/{currentTarget}"))
            {
                Debug.LogError("Asset Bundle directory doesn't exist! Creating directory!");
                Directory.CreateDirectory($"{ASSET_BUNDLE_PATH}/{currentTarget}");
            }

            BuildTarget target = System.Enum.TryParse<BuildTarget>(currentTarget, out var res) ?
                res : EditorUserBuildSettings.activeBuildTarget;
            var manifest = BuildPipeline.BuildAssetBundles($"{ASSET_BUNDLE_PATH}/{currentTarget}", buildMap,
                BuildAssetBundleOptions.None, target);

            var files = Directory.GetFiles($"{ASSET_BUNDLE_PATH}/{currentTarget}");
            for (int i = 0; i < files.Length; ++i)
            {
                AssetDatabase.ImportAsset(files[i], ImportAssetOptions.ForceUpdate);
            }

            GenerateVersions(currentTarget);
            SetUpEditorAssets(currentTarget);

            return GenerateNames(objects);
        }

        private static void SetUpEditorAssets(string currentTarget)
        {
            var bundles = Directory.GetFiles($"{ASSET_BUNDLE_PATH}/{currentTarget}")
                .Where(x => !Path.GetFileName(x).Equals(currentTarget) && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            if (!Directory.Exists($"{STREAMING_ASSETS_PATH}"))
            {
                Debug.LogError("Streaming Asset Bundle directory doesn't exist! Creating directory!");
                Directory.CreateDirectory($"{STREAMING_ASSETS_PATH}");
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

        private static bool GenerateNames(List<Container> objects)
        {
            var currentNames = File.ReadAllText($"{ASSET_BUNDLE_PATH}/names.json");

            var json = AssetBundlesNamesCreator.CreateNames(objects);
            File.WriteAllText($"{ASSET_BUNDLE_PATH}/names.json", json);
            AssetDatabase.ImportAsset($"{ASSET_BUNDLE_PATH}/names.json", ImportAssetOptions.ForceUpdate);

            return currentNames.Equals(json);
        }

        private static void GenerateVersions(string currentTarget)
        {
            var files = Directory.GetFiles($"{ASSET_BUNDLE_PATH}/{currentTarget}")
                .Where(x => !Path.GetFileName(x).Equals(currentTarget) && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            Dictionary<string, string> tokens = new Dictionary<string, string>();
            for (int i = 0; i < files.Length; ++i)
            {
                var bytes = File.ReadAllBytes(files[i]);
                if(!tokens.ContainsKey(files[i]))
                {
                    tokens.Add(Path.GetFileName(files[i]), Md5Sum(bytes));
                }
                else
                {
                    Debug.LogWarning($"{files[i]} already in dictionary");
                }
            }

            var json = JsonConvert.SerializeObject(tokens, Formatting.Indented);
            File.WriteAllText($"{ASSET_BUNDLE_PATH}/{currentTarget}/version.json", json);
            AssetDatabase.ImportAsset($"{ASSET_BUNDLE_PATH}/{currentTarget}/version.json", ImportAssetOptions.ForceUpdate);
        }

        private static string Md5Sum(byte[] bytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            return hashString.PadLeft(32, '0');
        }
    }
}
