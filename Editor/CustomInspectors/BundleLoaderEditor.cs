using UnityEditor;
using System.Collections.Generic;
using BundlesLoader.Bundles.Loaders;
using System.Linq;
using System;

namespace BundlesLoader.CustomInspectors
{
    [CustomEditor(typeof(BundleLoader), true)]
    public abstract class BundleLoaderEditor : Editor
    {
        protected SerializedProperty fullPathProperty;
        protected SerializedProperty bundleNameProperty;
        protected SerializedProperty rootNameProperty;
        protected SerializedProperty entityNameProperty;

        public int index;
        protected string[] Names { get; set; }

        protected abstract string[] SetNames();
        protected abstract Tuple<string, string, string> GetData();

        protected virtual void OnEnable()
        {
            var bundleType = serializedObject.FindProperty("bundleType");
            fullPathProperty = bundleType.FindPropertyRelative("FullName");
            bundleNameProperty = bundleType.FindPropertyRelative("BundleName");
            entityNameProperty = bundleType.FindPropertyRelative("EntityName");
            rootNameProperty = bundleType.FindPropertyRelative("RootName");

            Names = SetNames();
            index = Names.ToList().IndexOf(fullPathProperty.stringValue);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawInspector();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawInspector()
        {
            List<string> elements = new List<string>();
            if(Names != null)
            {
                for (int i = 0; i < Names.Length; i++)
                {
                    elements.Add(Names[i]);
                }
                index = EditorGUILayout.Popup(index, elements.ToArray());
                if (index < elements.Count)
                {
                    var elementAtIndex = elements[index];
                    if (elementAtIndex != null)
                    {
                        var elem = elements[index];
                        fullPathProperty.stringValue = elem;

                        var data = GetData();
                        rootNameProperty.stringValue = data.Item1;
                        bundleNameProperty.stringValue = data.Item2;
                        entityNameProperty.stringValue = data.Item3;
                    }
                }
            }
        }
    }
}
