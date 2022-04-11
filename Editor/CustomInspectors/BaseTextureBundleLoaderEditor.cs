using UnityEditor;
using Utils;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using BundlesLoader.Bundles.Loaders;

namespace BundlesLoader.CustomInspectors
{
    public class BaseTextureBundleLoaderEditor<T> : BundleLoaderEditor where T: TextureBundleLoader
    {
        private Sprite currentSprite;
        private Sprite initSprite;

        private readonly List<string> EXTENSIONS = new List<string>() { ".png", ".jpg" };

        protected override void OnEnable()
        {
            base.OnEnable();
            initSprite = GetCurrentSprite();
        }

        private Sprite GetCurrentSprite()
        {
            var obj = fullPathProperty.serializedObject.targetObject as T;
            if (obj != null)
            {
                var img = obj.GetComponent<Image>();
                if (img != null)
                {
                    return img.sprite;
                }
            }

            return null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawInspector();

            var split = fullPathProperty.stringValue.Split('/');
            if(split.Length != 3)
            {
                return;
            }

            var sprite = AssetLoader.GetAsset<Sprite>(
                split[1],
                split[2]);

            if(sprite == null)
            {
                Debug.LogError($"No sprite to show: {fullPathProperty.stringValue}");
                return;
            }

            if(currentSprite != sprite)
            {
                currentSprite = sprite;
                SetSprite(currentSprite);
            }

            if (currentSprite != null)
            {
                EditorGUILayout.BeginHorizontal();
                TextureField("Preview: ", currentSprite.texture);
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            SetSprite(initSprite);
        }

        private Texture2D TextureField(string name, Texture2D texture)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fixedWidth = 70
            };
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(100));
            GUILayout.EndVertical();
            return result;
        }

        private void SetSprite(Sprite value)
        {
            var obj = fullPathProperty.serializedObject.targetObject as T;
            if (obj != null)
            {
                var img = obj.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = value;
                }
            }
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
            var obj = fullPathProperty.serializedObject.targetObject as T;
            if (obj == null)
            {
                Debug.LogError($"No target object with specified type: {nameof(T)}");
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
                strs.Add("No textures elements inside bundles!");

            return strs.ToArray();
        }
    }
}
