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
            var assetType = property.GetValue<AssetType>();

            if (assetType != null)
            {
                var val = assetType.Paths?.FullPath ?? string.Empty;

                Set(val);
                index = EditorGUI.Popup(position, property.displayName, index, names.ToArray());
                if (index != -1 && index < names.Count && names[index] != val)
                {
                    var type = new AssetType(GetPathComponents(names[index]));
                    property.SetValue(type);
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