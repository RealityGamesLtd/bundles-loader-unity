namespace BundlesLoader.Callbacks
{
    public enum BundleErrorType
    {
        NO_BUNDLE,
        EMPTY_BUNDLE,
        NULL_BUNDLE,
        FAILED
    }

    public enum RetrieverType
    {
        ONLINE,
        OFFLINE,
        LOADER
    }

    public struct BundleCallback : IEntityCallback
    {
        public BundleCallback(RetrieverType retrieverType, BundleErrorType callbackType, string message, string bundleName)
        {
            RetrieverType = retrieverType;
            CallbackType = callbackType;
            Message = message;
            BundlePath = bundleName;
        }

        public RetrieverType RetrieverType { get; private set; }
        public BundleErrorType CallbackType { get; private set; }
        public string Message { get; private set; }
        public string BundlePath { get; private set; }
    }
}
