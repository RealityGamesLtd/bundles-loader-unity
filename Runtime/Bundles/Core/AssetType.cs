using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class AssetType
    {
        [SerializeField] public PathComponent Paths;

        public AssetType(PathComponent pathComponents)
        {
            Paths = pathComponents;
        }

        public override string ToString()
        {
            return Paths.FullPath;
        }
    }

    /// <summary>
    /// Path has 4 elements: Bundles/{BundleName}/{SpriteAtlasName}/{AssetName}
    /// </summary>
    [System.Serializable]
    public class SpriteAtlasAssetPathComponent : PathComponent
    {
        public string SpriteAtlasName { get; set; }

        public SpriteAtlasAssetPathComponent(string path)
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            Type = EntityType.ATLAS;
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

            Type = EntityType.ATLAS;
            RootName = "Bundles";
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
    [System.Serializable]
    public class AssetPathComponent : PathComponent
    {
        public AssetPathComponent(string path)
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            Type = EntityType.STANDALONE;
            RootName = splitString[0];
            BundleName = splitString[1];
            AssetName = splitString[2];
            FullPath = path;
        }

        public AssetPathComponent(string bundleName, string assetName)
        {
            var path = $"Bundles/{bundleName}/{assetName}";

            if (IsValidPath(path) == false) return;

            Type = EntityType.STANDALONE;
            RootName = "Bundles";
            BundleName = bundleName;
            AssetName = assetName;
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
