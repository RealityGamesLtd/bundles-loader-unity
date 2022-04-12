using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class AssetType
    {
        [SerializeField] public PathComponent Paths;
        [HideInInspector]
        public string FullPath;

        public AssetType(PathComponent pathComponents)
        {
            Paths = pathComponents;
        }

        public override string ToString()
        {
            return Paths.FullPath;
        }
    }

    [System.Serializable]
    public class PathComponent
    {
        public string RootName;
        public string BundleName;
        public string AssetName;
        public string FullPath;
    }

    /// <summary>
    /// Path has 4 elements: Bundles/{BundleName}/{SpriteAtlasName}/{AssetName}
    /// </summary>
    public class SpriteAtlasAssetPathComponent : PathComponent
    {
        public string SpriteAtlasName { get; set; }

        public SpriteAtlasAssetPathComponent(string path)
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            RootName = splitString[0];
            BundleName = splitString[1];
            SpriteAtlasName = splitString[2];
            AssetName = splitString[3];

            FullPath = path;
        }

        public SpriteAtlasAssetPathComponent(string bundleName, string spriteAtlasName, string assetName)
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
            return count == 3;
        }
    }

    /// <summary>
    /// Path has 3 elements: Bundles/{BundleName}/{AssetName}
    /// </summary>
    public class AssetPathComponent : PathComponent
    {
        public AssetPathComponent(string path)
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
            return count == 2;
        }
    }
}
