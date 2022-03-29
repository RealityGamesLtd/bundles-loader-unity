using System.Collections.Generic;
using System.Linq;
using Bundles.Utils;
using BundlesLoader.Bundles.Core;
using UnityEditor;
using UnityEngine;

namespace BundlesLoader.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AssetType))]
    public class AssetTypeDrawer : PropertyDrawer
    {
        private int index = 0;
        private readonly List<string> names = new List<string>();

        public AssetTypeDrawer() : base()
        {
            names = AssetTreeNames.Initialize(AssetBundlesChecker.GetBundlesNames()).ToList();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fullPath  = property.FindPropertyRelative("FullPath");
            var targetObject = property.serializedObject.targetObject;

            if(fullPath != null)
            {
                Set(fullPath.stringValue);
                index = EditorGUI.Popup(position, property.name, index, names.ToArray());
                if (index != -1 && index < names.Count && names[index] != fullPath.stringValue)
                {
                    fullPath.stringValue = names[index];
                    EditorUtility.SetDirty(targetObject);
                }
            }
        }

        private void Set(string name)
        {
            index = names.IndexOf(name);
        }
    }
}