using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils;
using Utils;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    public enum ElementType
    {
        DIRECTORY,
        FILE
    }

    public class Element
    {
        public ElementType Type { get; private set; }
        public string Name { get; private set; }

        public Element(ElementType type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class AssetBundlesPreprocessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private readonly string TEMP_PATH = $"{Application.persistentDataPath}/Temp";
        private readonly string TEXTURES_PATH = $"{Application.dataPath}/Textures";

        public int callbackOrder { get { return 0; } }
        private List<Element> savedNames = new List<Element>();

        public void OnPreprocessBuild(BuildReport report)
        {
            var assetBundlesPath = $"{Symbols.ASSET_BUNDLE_PATH}/{EditorUserBuildSettings.activeBuildTarget}";

            if (Directory.Exists(assetBundlesPath))
            {
                var files = Directory.GetFiles(assetBundlesPath).ToList();

                var bundles = files.Where(x => !Path.GetFileName(x).Equals(EditorUserBuildSettings.activeBuildTarget.ToString())
                    && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();
                var versionFile = files.Find(x => Path.GetExtension(x).Equals(".json"));

                if (bundles != null && versionFile != null)
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

            if (!Directory.Exists(TEXTURES_PATH))
            {
                Debug.LogError($"{TEXTURES_PATH} doesn't exists!");
                return;
            }

            if (!Directory.Exists(TEMP_PATH))
            {
                Debug.LogWarning($"{TEMP_PATH} doesn't exists! Creating directory");
                Directory.CreateDirectory(TEMP_PATH);
            }

            DirectoryInfo di = new DirectoryInfo(TEMP_PATH);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            savedNames.Clear();
            var texFiles = Directory.GetFiles(TEXTURES_PATH).ToList();
            var directories = Directory.GetDirectories(TEXTURES_PATH).ToList();

            texFiles = texFiles.Where(x =>
                Regex.Match(Path.GetExtension(x), AssetsRegexs.TEXTURE_REGEX).Success || Regex.Match(Path.GetExtension(x), AssetsRegexs.META_REGEX).Success).ToList();

            foreach (var file in texFiles)
            {
                var info = new FileInfo(file);
                info?.MoveTo($"{TEMP_PATH}/{info.Name}");

                savedNames.Add(new Element(ElementType.FILE, info.Name));
                AssetDatabase.DeleteAsset(file);
            }

            foreach (var directory in directories)
            {
                var info = new DirectoryInfo(directory);
                var metaFile = new FileInfo($"{directory}.meta");
                metaFile?.Delete();
                info?.MoveTo($"{TEMP_PATH}/{info.Name}");

                savedNames.Add(new Element(ElementType.DIRECTORY, info.Name));
                AssetDatabase.DeleteAsset(directory);
                AssetDatabase.DeleteAsset($"{directory}.meta");
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (!Directory.Exists(TEMP_PATH))
            {
                Debug.LogError($"{TEMP_PATH} doesn't exists!");
                return;
            }

            if (!Directory.Exists(TEXTURES_PATH))
            {
                Debug.LogError($"{TEXTURES_PATH} doesn't exists!");
                return;
            }

            foreach (var content in savedNames)
            {
                var path = $"{TEXTURES_PATH}/{content.Name}";
                FileUtil.MoveFileOrDirectory($"{TEMP_PATH}/{content.Name}", path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            AssetDatabase.Refresh();
        }
    }
}