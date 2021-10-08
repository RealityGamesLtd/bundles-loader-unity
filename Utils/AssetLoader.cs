using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Utils
{
    public class AssetLoader
    {
        public static T GetAsset<T>(string assetBundleName, string assetName)
            where T : Object
        {
#if UNITY_EDITOR
            if (!Directory.Exists(Symbols.ASSET_BUNDLE_PATH))
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
#else 
            return null;
#endif
        }
    }
}