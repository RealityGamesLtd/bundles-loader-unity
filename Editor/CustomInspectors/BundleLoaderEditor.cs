using UnityEditor;
using System.Collections.Generic;
using BundlesLoader.Bundles.Loaders;
using System.Linq;

namespace BundlesLoader.CustomInspectors
{
    [CustomEditor(typeof(BundleLoader), true)]
    public abstract class BundleLoaderEditor : Editor
    {
        protected SerializedProperty currentPath;

        public int index;
        protected string[] Names { get; set; }

        protected abstract string[] SetNames();

        protected virtual void OnEnable()
        {
            var bundleType = serializedObject.FindProperty("bundleType");
            currentPath = bundleType.FindPropertyRelative("FullName");
            Names = SetNames();

            index = Names.ToList().IndexOf(currentPath.stringValue);
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
                        currentPath.stringValue = elem;
                    }
                }
            }
        }
    }
}
