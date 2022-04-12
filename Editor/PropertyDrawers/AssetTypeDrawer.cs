using System.Collections.Generic;
using System.Linq;
using Bundles.Utils;
using BundlesLoader.Bundles.Core;
using BundlesLoader.Utils;
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
            var targetObject = property.serializedObject.targetObject;
            var fullPath = property.FindPropertyRelative("FullPath");

            if (property.GetValue() is AssetType assetType && fullPath != null)
            {
                var val = assetType.Paths?.FullPath ?? fullPath.stringValue ?? string.Empty;
                Set(val);
                index = EditorGUI.Popup(position, property.displayName, index, names.ToArray());
                if (index != -1 && index < names.Count && names[index] != val)
                {
                    fullPath.stringValue = names[index];
                    property.SetValue(new AssetType(GetPathComponents(names[index])));
                    EditorUtility.SetDirty(targetObject);
                }
            }
        }

        public PathComponent GetPathComponents(string path)
        {
            if (AssetPathComponent.IsValidPath(path)) return new AssetPathComponent(path);
            if (SpriteAtlasAssetPathComponent.IsValidPath(path)) return new SpriteAtlasAssetPathComponent(path);
            return null;
        }

        private void Set(string name)
        {
            index = names.IndexOf(name);
        }
    }
}