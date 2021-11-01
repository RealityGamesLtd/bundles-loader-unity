using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private List<string> names = new List<string>();

        public AssetTypeDrawer() : base()
        {
            names = AssetTreeNames.Initialize(AssetBundlesChecker.GetBundlesNames()).ToList();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetObject = property.serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();

            var bindingFlags = BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.NonPublic |
                BindingFlags.Public;
            var field = targetObjectClassType.GetField(property.propertyPath, bindingFlags);
            if (field != null && names.Count > 0)
            {
                if (field.GetValue(targetObject) is AssetType value)
                {
                    Set(value.FullPath);
                    index = EditorGUI.Popup(position, property.propertyPath, index, names.ToArray());
                    if (index != -1  && index < names.Count && names[index] != value.FullPath)
                    {
                        value.FullPath = names[index];
                        EditorUtility.SetDirty(targetObject);
                    }
                }
            }
        }

        private void Set(string name)
        {
            index = names.IndexOf(name);
        }
    }
}