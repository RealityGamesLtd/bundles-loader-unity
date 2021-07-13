using System.Collections.Generic;
using System.Linq;
using BundlesLoader.Bundles.Loaders;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Utils;

namespace BundlesLoader.CustomInspectors
{
    public abstract class BaseSpriteBundleLoaderEditor<T> : BundleLoaderEditor where T: SpriteAtlasBundleLoader
    {
        private Sprite currentSprite;
        private Sprite initSprite;

        private readonly List<string> EXTENSIONS = new List<string>(){ ".spriteatlas" };

        protected override void OnEnable()
        {
            base.OnEnable();
            initSprite = GetCurrentSprite();
        }

        private Sprite GetCurrentSprite()
        {
            var obj = currentPath.serializedObject.targetObject as T;
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

            var split = currentPath.stringValue.Split('/');
            if(split.Length != 4)
            {
                return;
            }

            var spriteAtlas = AssetLoader.GetAsset<SpriteAtlas>(
                split[1],
                split[2]);

            if(spriteAtlas == null)
            {
                Debug.LogError($"No sprite atlas found: {currentPath.stringValue}");
                return;
            }

            var sprite = spriteAtlas.GetSprite(split[3]);

            if (sprite == null)
            {
                Debug.LogError($"No sprite to show: {currentPath.stringValue}");
                return;
            }

            if (currentSprite != sprite)
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
            var obj = currentPath.serializedObject.targetObject as T;
            if(obj != null)
            {
                obj.SetSprite(value);
            }
        }

        protected override string[] SetNames()
        {
            var obj = currentPath.serializedObject.targetObject as T;
            if(obj == null)
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
                strs.Add("No sprite atlases inside bundles!");

            return strs.ToArray();
        }
    }
}
