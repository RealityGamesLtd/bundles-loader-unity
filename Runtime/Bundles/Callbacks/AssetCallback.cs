namespace BundlesLoader.Callbacks
{
    public enum AssetErrorType
    {
        NULL_SPRITEATLAS,
        NULL_SPRITE,
        NULL_TEXTURE,
        NULL_GIF
    }

    public struct AssetCallback : IEntityCallback
    {
        public AssetCallback(AssetErrorType callbackType, string message, string bundlePath, string assetName)
        {
            CallbackType = callbackType;
            Message = message;
            BundlePath = bundlePath;
            AssetName = assetName;
        }

        public AssetErrorType CallbackType { get; private set; }
        public string Message { get; private set; }
        public string BundlePath { get; private set; }
        public string AssetName { get; private set; }
    }
}
