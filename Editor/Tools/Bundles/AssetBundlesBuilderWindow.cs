using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utils;

namespace BundlesLoader.EditorHelpers.Tools.Bundles
{
    [System.Serializable]
    public class Container
    {
        public string BundleName;
        public List<Object> SelectedObjects;
    }

    public class AssetBundleBuilderWindow : EditorWindow
    {
        [SerializeField] private List<Container> selectedObjects;

        private Vector2 scrollPos;
        private bool freshBuild;
        private int index = 0;
        private int minVersion = 0;
        private int maxVersion = 0;
        private string[] targets;
        private string[] versions;
        private Versionable versionable;

        [MenuItem("Window/Asset Bundle Builder")]
        static void ShowWindow()
        {
            AssetBundleBuilderWindow window = (AssetBundleBuilderWindow)GetWindow(typeof(AssetBundleBuilderWindow), false, "Asset Bundle Builder");
            window.maxSize = new Vector2(400f, 495f);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }

        private void OnEnable()
        {
            var ls = new List<string>()
            {
                BuildTarget.Android.ToString(),
                BuildTarget.iOS.ToString(),
            };
            targets = ls.ToArray();
        }

        private void OnGUI()
        {
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty selectedContainers = so.FindProperty("selectedObjects");
            index = EditorGUILayout.Popup("Platform:", index, targets);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            EditorGUILayout.LabelField("Versions", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            versionable = (Versionable)EditorGUILayout.ObjectField("Versions file: ", versionable, typeof(Versionable), false);
            if (versionable != null)
            {
                versions = versionable.Versions.ToArray();
                minVersion = EditorGUILayout.Popup("Min Version:", minVersion, versions);
                maxVersion = EditorGUILayout.Popup("Max Version:", maxVersion, versions);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("Selected objects", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.PropertyField(selectedContainers, true);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            so.ApplyModifiedProperties();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("Load"))
            {
                LoadBundles();
            }
            if (versionable != null && GUILayout.Button("Build"))
            {
                if(!AssetBundleBuilder.BuildBundles(targets[index], selectedObjects, freshBuild,
                    versions[minVersion], versions[maxVersion]))
                {
                    EditorUtility.DisplayDialog("Asset Bundle Builder", "Asset Bundles are not consistent with each other!", "Ok");
                }
            }
            freshBuild = EditorGUILayout.Toggle("Create fresh build: ", freshBuild);
            EditorGUILayout.EndVertical();
            Repaint();
        }

        private async Task LoadBundles()
        {
            if (!Directory.Exists($"{Symbols.ASSET_BUNDLE_PATH}/{targets[index]}"))
            {
                EditorUtility.DisplayDialog("Asset Bundle Builder", "Asset Bundle directory doesn't exist!", "Ok");
                Debug.LogError("Asset Bundle directory doesn't exist!");
                return;
            }

            var files = Directory.GetFiles($"{Symbols.ASSET_BUNDLE_PATH}/{targets[index]}")
                .Where(x => !Path.GetFileName(x).Equals(targets[index]) && string.IsNullOrEmpty(Path.GetExtension(x))).ToArray();

            if (files.Length <= 0)
            {
                EditorUtility.DisplayDialog("Asset Bundle Builder", "No saved asset bundles!", "Ok");
                Debug.LogWarning("No saved asset bundles!");
                return;
            }

            selectedObjects = new List<Container>();

            for (int i = 0; i < files.Length; ++i)
            {
                var loadFileTask = AssetBundle.LoadFromFileAsync(files[i]);
                while (!loadFileTask.isDone)
                    await Task.Yield();

                var paths = loadFileTask.assetBundle.GetAllAssetNames();
                loadFileTask.assetBundle.Unload(true);

                Object[] objects = new Object[paths.Length];

                for (int j = 0; j < objects.Length; ++j)
                {
                    objects[j] = AssetDatabase.LoadAssetAtPath(paths[j], typeof(Object));
                }
                var container = new Container()
                {
                    BundleName = Path.GetFileName(files[i]),
                    SelectedObjects = objects.ToList()
                };

                selectedObjects.Add(container);
            }
        }
    }
}
