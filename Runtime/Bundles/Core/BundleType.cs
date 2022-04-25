using Bundles.Utils;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class BundleType
    {
        public string[] Names => AssetTreeNames.Initialize(AssetBundlesChecker.GetBundlesNames());
        public string FullName;
        public string RootName;
        public string BundleName;
        public string EntityName;
        public EntityType Type;
    }
}
