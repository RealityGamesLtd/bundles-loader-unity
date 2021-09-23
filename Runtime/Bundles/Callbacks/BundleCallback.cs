namespace BundlesLoader.Callbacks
{
    public enum BundleErrorType
    {
        NO_BUNDLE,
        EMPTY_BUNDLE,
        NULL_BUNDLE,
        FAILED
    }

    public struct BundleCallback : IEntityCallback
    {
        public BundleCallback(BundleErrorType callbackType, string message, string bundleName)
        {
            CallbackType = callbackType;
            Message = message;
            BundlePath = bundleName;
        }

        public BundleErrorType CallbackType { get; private set; }
        public string Message { get; private set; }
        public string BundlePath { get; private set; }
    }
}
