using UnityEditor;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools
{
    public class InfoWindow : EditorWindow
    {
        private Texture2D currentTexture;
        private Vector2 currentSize;

        public void Initialize(Vector2 size, string title, Texture2D tex)
        {
            currentTexture = tex;
            currentSize = size;

            InfoWindow window = (InfoWindow)GetWindow(typeof(InfoWindow), false, title);
            window.maxSize = size;
            window.minSize = window.maxSize;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Box(currentTexture, GUILayout.Width(currentSize.x - 10f), GUILayout.Height(currentSize.y));
            EditorGUILayout.EndVertical();
        }
    }
}
