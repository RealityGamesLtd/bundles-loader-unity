using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class AssetType
    {
        [HideInInspector]
        public string FullName;

        public IPathComponents GetPathComponents()
        {
            if (AssetPathComponents.IsValidPath(FullName)) return new AssetPathComponents(FullName);
            if (SpriteAtlasAssetPathComponents.IsValidPath(FullName)) return new SpriteAtlasAssetPathComponents(FullName);
            return null;
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    public interface IPathComponents
    {
        string BundleName { get; }
        string AssetName { get; }
    }

    /// <summary>
    /// Path has 4 elements: Bundles/{BundleName}/{SpriteAtlasName}/{AssetName}
    /// </summary>
    public struct SpriteAtlasAssetPathComponents : IPathComponents
    {
        public string BundleName { get; set; }
        public string SpriteAtlasName { get; set; }
        public string AssetName { get; set; }

        public SpriteAtlasAssetPathComponents(string path) : this()
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            BundleName = splitString[1];
            SpriteAtlasName = splitString[2];
            AssetName = splitString[3];
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            var splitString = path.Split('/');
            return splitString.Length == 4;
        }
    }

    /// <summary>
    /// Path has 3 elements: Bundles/{BundleName}/{AssetName}
    /// </summary>
    public struct AssetPathComponents : IPathComponents
    {
        public string BundleName { get; set; }
        public string AssetName { get; set; }

        public AssetPathComponents(string path) : this()
        {
            if (IsValidPath(path) == false) return;

            var splitString = path.Split('/');

            BundleName = splitString[1];
            AssetName = splitString[2];
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            var splitString = path.Split('/');
            return splitString.Length == 3;
        }
    }
}
