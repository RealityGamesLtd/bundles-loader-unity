using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class AssetLoader
    {
        private const string ASSET_BUNDLE_PATH = "Assets/AssetBundles";

        public static T GetAsset<T>(string assetBundleName, string assetName)
            where T : Object
        {
            if (!Directory.Exists(ASSET_BUNDLE_PATH))
            {
                Debug.LogError("No directory found!");
                return null;
            }

            var paths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            if(paths == null)
            {
                Debug.LogError($"No asset bundle specified name: {assetBundleName}");
                return null;
            }

            var assetPath = paths.ToList().Find(x => x.Contains(assetName));

            if(assetPath == null)
            {
                Debug.LogError($"No asset with specified asset name: {assetName}");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }
}


