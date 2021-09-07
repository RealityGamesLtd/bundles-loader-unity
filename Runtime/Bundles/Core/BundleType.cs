using Bundles.Utils;
using UnityEngine;

namespace BundlesLoader.Bundles.Core
{
    [System.Serializable]
    public class BundleType
    {
        public string[] Names => AssetTreeNames.Initialize(AssetBundlesChecker.GetBundlesNames());
        [HideInInspector]
        public string FullName;
    }
}
