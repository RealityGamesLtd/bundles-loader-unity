using System.Collections.Generic;
using UnityEngine;

namespace BundlesLoader.Service
{
    public static class PlatformDictionary
    {
#if UNITY_EDITOR
        public readonly static Dictionary<UnityEditor.BuildTarget, string> BuildPlatforms = new Dictionary<UnityEditor.BuildTarget, string>()
        {
            { UnityEditor.BuildTarget.Android, "android"},
            { UnityEditor.BuildTarget.iOS, "ios" }
        };
#endif
        public readonly static Dictionary<RuntimePlatform, string> RuntimePlatforms = new Dictionary<RuntimePlatform, string>()
        {
            {RuntimePlatform.Android, "android"},
            {RuntimePlatform.IPhonePlayer, "ios" }
        };

        public static string GetDirectoryByPlatform(RuntimePlatform platform)
        {
            string output = string.Empty;

            if (!Application.isEditor)
            {
                if (RuntimePlatforms.TryGetValue(platform, out var platf))
                {
                    output = platf;
                }
            }
            else
            {
#if UNITY_EDITOR
                if (BuildPlatforms.TryGetValue(UnityEditor.EditorUserBuildSettings.activeBuildTarget, out var platf))
                {
                    output = platf;
                }
#endif
            }
            return output;
        }
    }
}
