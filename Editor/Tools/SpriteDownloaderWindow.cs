using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEditor.U2D;

namespace BundlesLoader.EditorHelpers.Tools
{
    public class SpriteDownloaderWindow : EditorWindow
    {
        private const string APP_NAME = "Drive API .NET Quickstart";
        private const string PACKAGE_NAME = "bundles-loader";

        private const string TEXTURES_PATH = "Assets/Textures/spritesheets";

        public class TexturePack
        {
            public TexturePack(Texture2D texture, string name, string parent)
            {
                Texture = texture;
                Name = name;
                Parent = parent;
            }

            public Texture2D Texture { get; private set; }
            public string Name { get; private set; }
            public string Parent { get; private set; }
        }

        private enum AssetType
        {
            Single,
            SinglePacked
        }

        private readonly UnityEvent OnDownloaded = new UnityEvent();
        private readonly UnityEvent OnSaved = new UnityEvent();
        private readonly string[] SCOPES = { DriveService.Scope.DriveReadonly };

        private AssetType currentType = AssetType.Single;
        private string driveFolderID;
        private int rows, cols;
        private Vector2 scrollPos;
        private TexturePack singleTex;
        private TexturePack[] multipleTexs;
        private DriveService driveService;

        [MenuItem("Window/Sprite Downloader")]
        static void ShowWindow()
        {
            SpriteDownloaderWindow window = (SpriteDownloaderWindow)GetWindow(typeof(SpriteDownloaderWindow), false, "Sprite Downloader");
            window.maxSize = new Vector2(400f, 440f);
            window.minSize = window.maxSize;
            window.Show();
        }

        private void Awake()
        {
            UserCredential credential;
            var currDir = Directory.GetCurrentDirectory();
            using (var stream =
                new FileStream($"{currDir}/{PACKAGE_NAME}/credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Application.persistentDataPath + "/token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    SCOPES,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = APP_NAME,
            });
        }

        private void OnGUI()
        {
            currentType = (AssetType)EditorGUILayout.EnumPopup("Asset type:", currentType);
            EditorGUILayout.Space(20);

            OnDownloaded.RemoveAllListeners();
            OnSaved.RemoveAllListeners();

            switch (currentType)
            {
                case AssetType.SinglePacked:
                    DrawSingle();
                    break;
                case AssetType.Single:
                    DrawMultiple();
                    break;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("Download"))
            {
                OnDownloaded?.Invoke();
            }
            if (GUILayout.Button("Save"))
            {
                OnSaved?.Invoke();
            }
            EditorGUILayout.EndVertical();

            GUILayout.BeginArea(new Rect(400f - 28f, 22f, 25f, 35f));
            Texture2D play = EditorGUIUtility.FindTexture("_Help");
            if (GUILayout.Button(play))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Tools/info.jpg");
                InfoWindow window = (InfoWindow)GetWindow(typeof(InfoWindow), false, "Sprite Downloader Info");
                window.Initialize(new Vector2(300f, 260f), "Sprite Downloader Info", texture);
            }
            GUILayout.EndArea();

            Repaint();
        }

        private void DrawMultiple()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            driveFolderID = EditorGUILayout.TextField("Folder ID: ", driveFolderID);
            EditorGUILayout.EndVertical();

            OnDownloaded.AddListener(async () => multipleTexs = await DownloadSpriteSheet());
            OnSaved.AddListener(async () => await SaveSpriteSheet(multipleTexs));

            if (multipleTexs != null)
            {
                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(392), GUILayout.Height(295));
                for(int i = 0; i < multipleTexs.Length; ++i)
                {
                    EditorGUILayout.LabelField($"{multipleTexs[i].Parent}/{multipleTexs[i].Name}", EditorStyles.miniBoldLabel);
                    GUILayout.Box(multipleTexs[i].Texture, GUILayout.Width(370), GUILayout.Height(250));
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawSingle()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            driveFolderID = EditorGUILayout.TextField("Folder ID: ", driveFolderID);

            rows = EditorGUILayout.IntSlider("Rows: ", rows, 1, 8);
            cols = EditorGUILayout.IntSlider("Columns: ", cols, 1, 8);

            EditorGUILayout.EndVertical();

            OnDownloaded.AddListener(async () => singleTex = await DownloadSingleSpritesToSpriteSheet());
            OnSaved.AddListener(async () => await SaveSpriteSheet(singleTex));

            if (singleTex != null)
            {
                EditorGUILayout.LabelField(singleTex.Name, EditorStyles.boldLabel);
                GUILayout.Box(singleTex.Texture, GUILayout.Width(392), GUILayout.Height(250));
            }
        }

        private async Task<TexturePack[]> DownloadSpriteSheet()
        {
            var parentFolder = driveService.Files.Get(driveFolderID).Execute();
            var folderName = parentFolder.Name;

            var listRequest = driveService.Files.List();
            listRequest.Q = $"'{driveFolderID}' in parents and mimeType = 'application/vnd.google-apps.folder'";
            var folders = listRequest.Execute().Files.ToArray();

            List<Task<(byte[], string FolderName, string Name)>> tasks = new List<Task<(byte[], string FolderName, string Name)>>();

            if (folders.Length <= 0)
            {
                Debug.LogWarning($"No folders in this directory: {folderName}, getting spritsheet from this directory!");
                tasks.AddRange(GetFilesFromFolder(parentFolder, folderName));
            }
            else
            {
                for (int i = 0; i < folders.Length; ++i)
                {
                    var subFolderName = folders[i].Name;
                    tasks.AddRange(GetFilesFromFolder(folders[i], subFolderName));
                }
            }


            (byte[] Bytes, string FolderName, string Name)[] resp;

            try
            {
                resp = await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }

            if (resp == null)
            {
                Debug.LogError("Bytes are null!");
                return null;
            }

            TexturePack[] textures = new TexturePack[resp.Length];
            for (int i = 0; i < resp.Length; ++i)
            {
                var tex = new Texture2D(1, 1);
                tex.LoadImage(resp[i].Bytes);

                textures[i] = new TexturePack(tex, resp[i].Name, resp[i].FolderName);
            }
            return textures;
        }

        private IEnumerable<Task<(byte[], string FolderName, string Name)>> GetFilesFromFolder(
            Google.Apis.Drive.v3.Data.File folder, string subFolderName)
        {
            var listRequest = driveService.Files.List();
            listRequest.Q = $"('{folder.Id}' in parents)";
            var files = listRequest.Execute().Files.ToArray();
            if (files.Length <= 0)
            {
                Debug.LogWarning($"No files in this directory: {subFolderName}, omitting this directory!");
            }

            return files.Select(async x =>
            {
                var task = driveService.Files.Get(x.Id);
                using MemoryStream ms = new MemoryStream();
                await task.DownloadAsync(ms);
                return (ms.GetBuffer(), $"{subFolderName}", $"{x.Name}");
            });
        }

        private async Task<TexturePack> DownloadSingleSpritesToSpriteSheet()
        {
            var folderName = driveService.Files.Get(driveFolderID).Execute().Name;

            var listRequest = driveService.Files.List();
            listRequest.Q = $"('{driveFolderID}' in parents)";
            var files = listRequest.Execute().Files.ToArray();

            if(files.Length <= 0)
            {
                EditorUtility.DisplayDialog("Sprite Downloader",
                    $"No files in this directory: {folderName}, no spritesheet created!", "Ok");

                Debug.LogError($"No files in this directory: {folderName}, no spritesheet created!");
                return null;
            }

            var tasks = files.Select(async x =>
            {
                var task = driveService.Files.Get(x.Id);
                using MemoryStream ms = new MemoryStream();
                await task.DownloadAsync(ms);
                return (ms.GetBuffer(), x.Name);
            });

            (byte[] Bytes, string Name)[] resp;

            try
            {
                resp = await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }

            if (resp == null)
            {
                Debug.LogError("Bytes are null!");
                return null;
            }

            Texture2D[] textures = new Texture2D[resp.Length];
            for (int i = 0; i < resp.Length; ++i)
            {
                textures[i] = new Texture2D(1, 1);
                textures[i].LoadImage(resp[i].Bytes);
            }

            int widthOffset = textures.Max(x => x.width);
            int heightOffset = textures.Max(x => x.height);
            int height = rows * heightOffset;
            int width = cols * widthOffset;

            var background = new Color32[height * width];
            for(int i = 0; i < background.Length; ++i)
            {
                background[i] = new Color32(0, 0, 0, 0);
            }

            var tex = new Texture2D(width, height);
            tex.SetPixels32(background);

            int z = 0;
            for(int i = rows - 1; i >= 0; --i)
            {
                for(int j = 0; j < cols; ++j)
                {
                    if(z < textures.Length)
                    {
                        tex.SetPixels32(j * widthOffset, i * heightOffset, textures[z].width, textures[z].height, textures[z].GetPixels32());
                        z++;
                    }
                }
            }

            tex.Apply();
            return new TexturePack(tex, $"{folderName}.png", string.Empty);
        }

        private async Task SaveSpriteSheet(params TexturePack[] texs)
        {
            if (texs == null)
            {
                EditorUtility.DisplayDialog("Sprite Downloader",
                    "Texture is null", "Ok");
                Debug.LogError("Texture is null");
                return;
            }

            var paths = new string[texs.Length];

            for (int i = 0; i < texs.Length; ++i)
            {
                byte[] bytes = texs[i].Texture.EncodeToPNG();
                if (!Directory.Exists($"{TEXTURES_PATH}/{texs[i].Parent}"))
                {
                    Debug.LogWarning($"{TEXTURES_PATH}/{texs[i].Parent} directory doesn't exist! Creating a new one!");
                    Directory.CreateDirectory($"{TEXTURES_PATH}/{texs[i].Parent}");
                }

                var path = $"{TEXTURES_PATH}/{texs[i].Parent}/{texs[i].Name}";
                using (FileStream SourceStream = File.Open(path, FileMode.Create))
                {
                    SourceStream.Seek(0, SeekOrigin.End);
                    await SourceStream.WriteAsync(bytes, 0, bytes.Length);
                    SourceStream.Close();
                }
                path = $"{TEXTURES_PATH}/{texs[i].Parent}/{texs[i].Name}";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                paths[i] = path;
            }

            await Task.Delay(200);
            AssetDatabase.SaveAssets();

            var dict = texs.GroupBy(x => x.Parent).ToDictionary(k => k.Key, v => v.Select(x => x.Name).ToArray());
            SaveSpriteAtlas(dict);
        }

        private void SaveSpriteAtlas(Dictionary<string, string[]> textures)
        {
            foreach (var pair in textures)
            {
                SpriteAtlas sa = new SpriteAtlas();
                sa.SetPackingSettings(new SpriteAtlasPackingSettings() { enableTightPacking = false, enableRotation = false });
                AssetDatabase.CreateAsset(sa, $"{TEXTURES_PATH}/{pair.Key}/{pair.Key}.spriteatlas");
                Sprite[] sprites = new Sprite[pair.Value.Length];

                for(int i = 0; i < pair.Value.Length; ++i)
                {
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURES_PATH}/{pair.Key}/{pair.Value[i]}");
                }

                if (sprites != null)
                    SpriteAtlasExtensions.Add(sa, sprites);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
