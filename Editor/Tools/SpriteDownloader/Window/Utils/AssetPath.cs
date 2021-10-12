namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils
{
    public class AssetPath
    {
        public AssetPath(string texturePath, string parentName, string assetName)
        {
            TexturePath = texturePath;
            ParentName = parentName;
            AssetName = assetName;
        }

        public override string ToString()
        {
            return $"{TexturePath}/{ParentName}/{AssetName}";
        }

        public string TexturePath { get; private set; }
        public string ParentName { get; private set; }
        public string AssetName { get; private set; }
    }
}
