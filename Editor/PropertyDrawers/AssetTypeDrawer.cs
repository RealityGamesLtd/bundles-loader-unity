using System.Collections.Generic;
using System.Reflection;
using BundlesLoader.Bundles.Core;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AssetType))]
public class AssetTypeDrawer : PropertyDrawer
{
    int index = 0;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var names = new List<string>();
        var targetObject = property.serializedObject.targetObject;
        var targetObjectClassType = targetObject.GetType();

        var bindingFlags = BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.NonPublic |
            BindingFlags.Public;
        var field = targetObjectClassType.GetField(property.propertyPath, bindingFlags);
            if (field != null)
        {
            if (field.GetValue(targetObject) is AssetType value)
            {
                for (int x = 0; x < value.Names.Length; x++)
                {
                    names.Add(value.Names[x]);
                }
                index = EditorGUI.Popup(position, index, names.ToArray());

                if(index < names.Count)
                    value.FullName = names[index];
            }
        }
    }
}