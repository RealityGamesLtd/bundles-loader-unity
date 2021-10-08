using UnityEngine;

namespace Utils
{
    public static class Symbols
    {
        public static readonly string STREAMING_ASSET_BUNDLE_PATH = $"{Application.streamingAssetsPath}/{BUNDLES_SUBDIRECTORY}";
        public const string ASSET_BUNDLE_PATH = "Assets/AssetBundles";
        public const string TEXTURES_PATH = "Assets/Textures/spritesheets";
        public const string BUNDLES_SUBDIRECTORY = "Bundles";
        public const string VERSION_FILE_NAME = "version.json";
        public const string NAMES_FILE_NAME = "names.json";
    }
}