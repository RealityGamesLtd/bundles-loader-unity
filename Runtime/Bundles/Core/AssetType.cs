using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class AssetType
    {
        [HideInInspector]
        public string FullPath;

        public AssetType(IPathComponents pathComponents)
        {
            FullPath = pathComponents.FullPath;
        }

        public IPathComponents GetPathComponents()
        {
            if (AssetPathComponents.IsValidPath(FullPath)) return new AssetPathComponents(FullPath);
            if (SpriteAtlasAssetPathComponents.IsValidPath(FullPath)) return new SpriteAtlasAssetPathComponents(FullPath);
            return null;
        }

        public override string ToString()
        {
            return FullPath;
        }
    }

    public interface IPathComponents
    {
        string RootName { get; }
        string BundleName { get; }
        string AssetName { get; }
        string FullPath { get; }
    }

    /// <summary>
    /// Path has 4 elements: Bundles/{BundleName}/{SpriteAtlasName}/{AssetName}
    /// </summary>
    public struct SpriteAtlasAssetPathComponents : IPathComponents
    {
        public string RootName { get; set; }
        public string BundleName { get; set; }
        public string SpriteAtlasName { get; set; }
        public string AssetName { get; set; }
        public string FullPath { get; private set; }

        public SpriteAtlasAssetPathComponents(string path) : this()
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            RootName = splitString[0];
            BundleName = splitString[1];
            SpriteAtlasName = splitString[2];
            AssetName = splitString[3];

            FullPath = path;
        }

        public SpriteAtlasAssetPathComponents(string bundleName, string spriteAtlasName, string assetName) : this()
        {
            var path = $"Bundles/{bundleName}/{spriteAtlasName}.spriteatlas/{assetName}";

            if (IsValidPath(path) == false) return;

            BundleName = bundleName;
            SpriteAtlasName = spriteAtlasName;
            AssetName = assetName;
            FullPath = path;
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            int count = 0;
            for (int i = 0; i < path.Length; ++i)
                if (path[i] == '/') count++;
            return count == 4;
        }
    }

    /// <summary>
    /// Path has 3 elements: Bundles/{BundleName}/{AssetName}
    /// </summary>
    public struct AssetPathComponents : IPathComponents
    {
        public string RootName { get; set; }
        public string BundleName { get; set; }
        public string AssetName { get; set; }
        public string FullPath { get; private set; }

        public AssetPathComponents(string path) : this()
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            RootName = splitString[0];
            BundleName = splitString[1];
            AssetName = splitString[2];
            FullPath = path;
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            int count = 0;
            for(int i = 0; i < path.Length; ++i)
                if (path[i] == '/') count++;
            return count == 3;
        }
    }
}
