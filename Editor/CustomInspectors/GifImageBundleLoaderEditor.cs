using System.Collections.Generic;
using System.Linq;
using BundlesLoader.Bundles.Loaders;
using UnityEditor;
using UnityEngine;
using Utils;

namespace BundlesLoader.CustomInspectors
{
    [CustomEditor(typeof(GifBundleLoader))]
    public class GifImageBundleLoaderEditor : BundleLoaderEditor
    {
        private readonly List<string> EXTENSIONS = new List<string>() { ".bytes" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawInspector();

            var split = fullPathProperty.stringValue.Split('/');
            if (split.Length != 3)
            {
                return;
            }

            var textAsset = AssetLoader.GetAsset<TextAsset>(
                split[1],
                split[2]);

            if (textAsset == null)
            {
                Debug.LogError($"No text asset found: {fullPathProperty.stringValue}");
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected override System.Tuple<string, string, string> GetData()
        {
            var split = fullPathProperty.stringValue.Split('/');
            if (split.Length != 3)
            {
                return new System.Tuple<string, string, string>(string.Empty, string.Empty, string.Empty);
            }

            return new System.Tuple<string, string, string>(split[0], split[1], split[2]);
        }

        protected override string[] SetNames()
        {
            var obj = fullPathProperty.serializedObject.targetObject as GifBundleLoader;
            if (obj == null)
            {
                Debug.LogError($"No target object with specified type: {nameof(GifBundleLoader)}");
                return null;
            }

            var strs = new List<string>
            {
                string.Empty
            };

            var names = obj.BundleType.Names;
            if (names != null)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    if (EXTENSIONS.Any(x => name.Contains(x)))
                        strs.Add(name);
                }
            }

            if (strs.Count == 0)
                strs.Add("No text asset inside bundles!");

            return strs.ToArray();
        }
    }
}