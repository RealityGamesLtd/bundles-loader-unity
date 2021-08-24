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
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs;
using System.Text.RegularExpressions;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader
{
    public static class AssetsRegexs
    {
        public const string TEXTURE_REGEX = "[^\\s]+(.*?)\\.([jJ][pP][gG]|[jJ][pP][eE][gG]|[pP][nN][gG])$";
        public const string BYTE_REGEX = "[^\\s]+(.*?)\\.([gG][iI][fF])$";
    }

    public partial class SpriteDownloaderWindow : EditorWindow
    {
        private enum AssetType
        {
            Standalone,
            Spritesheet
        }

        private const string PACKAGE_NAME = "com.realitygames.bundlesloader";
        private const string TEXTURES_PATH = "Assets/Textures/spritesheets";

        private readonly UnityEvent OnDownloaded = new UnityEvent();
        private readonly UnityEvent OnSaved = new UnityEvent();
        private readonly string[] SCOPES = { DriveService.Scope.DriveReadonly };

        private AssetType currentType = AssetType.Spritesheet;
        private string driveFolderID;
        private Vector2 scrollPos;
        private Pack[] packs;
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
            using (var stream =
                new FileStream($"Packages/{PACKAGE_NAME}/credentials.json", FileMode.Open, FileAccess.Read))
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
                ApplicationName = PACKAGE_NAME,
            });
        }

        private void OnGUI()
        {
            currentType = (AssetType)EditorGUILayout.EnumPopup("Asset type:", currentType);
            EditorGUILayout.Space(20);

            OnDownloaded.RemoveAllListeners();
            OnSaved.RemoveAllListeners();

            DrawGUI();
            OnSaved.AddListener(async () => await SaveSpriteSheet(currentType));

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

        private void DrawGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            driveFolderID = EditorGUILayout.TextField("Folder ID: ", driveFolderID);
            EditorGUILayout.EndVertical();

            OnDownloaded.AddListener(async () =>
            {
                packs = await DownloadSpriteSheet();
            });

            if (packs != null)
            {
                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(392), GUILayout.Height(295));
                for(int i = 0; i < packs.Length; ++i)
                {
                    EditorGUILayout.LabelField($"{packs[i].Parent}/{packs[i].Name}", EditorStyles.miniBoldLabel);
                    if(packs[i] is TexturePack texPack)
                    {
                        GUILayout.Box(texPack.Texture, GUILayout.Width(370), GUILayout.Height(250));
                    }
                    EditorGUILayout.Space(10);
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
        }

        private async Task<Pack[]> DownloadSpriteSheet()
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

            Pack[] lclPcks = new Pack[resp.Length];
            for (int i = 0; i < resp.Length; ++i)
            {
                ParsePacks(resp[i], out lclPcks[i]);
            }
            return lclPcks;
        }

        private void ParsePacks((byte[] Bytes, string FolderName, string Name) resp, out Pack lclPcks)
        {
            if (Regex.Match(resp.Name, AssetsRegexs.TEXTURE_REGEX).Success)
            {
                var tex = new Texture2D(1, 1);
                tex.LoadImage(resp.Bytes);
                lclPcks = new TexturePack(tex, resp.Name, resp.FolderName);
            }
            else if (Regex.Match(resp.Name, AssetsRegexs.BYTE_REGEX).Success)
            {
                lclPcks = new BytePack(resp.Bytes, resp.Name, resp.FolderName);
            }
            else
            {
                lclPcks = null;
                //TODO: More regexes in the future
            }
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

        private async Task SaveSpriteSheet(AssetType type)
        {
            if (packs == null)
            {
                EditorUtility.DisplayDialog("Sprite Downloader",
                    "There are no textures to be saved", "Ok");
                Debug.LogError("Textures are null");
                return;
            }

            var paths = new string[packs.Length];

            for (int i = 0; i < packs.Length; ++i)
            {
                byte[] bytes = packs[i].Bytes;
                if (!Directory.Exists($"{TEXTURES_PATH}/{packs[i].Parent}"))
                {
                    Debug.LogWarning($"{TEXTURES_PATH}/{packs[i].Parent} directory doesn't exist! Creating a new one!");
                    Directory.CreateDirectory($"{TEXTURES_PATH}/{packs[i].Parent}");
                }

                if(Regex.Match(packs[i].Name, AssetsRegexs.BYTE_REGEX).Success)
                {
                    packs[i].Name = $"{Path.GetFileName(packs[i].Name)}.bytes";
                }

                var path = $"{TEXTURES_PATH}/{packs[i].Parent}/{packs[i].Name}";
                string metaContent = string.Empty;
                if (File.Exists($"{path}.meta"))
                {
                    metaContent = File.ReadAllText($"{path}.meta");
                    AssetDatabase.DeleteAsset(path);
                }

                using (FileStream SourceStream = File.Open(path, FileMode.Create))
                {
                    SourceStream.Seek(0, SeekOrigin.End);
                    await SourceStream.WriteAsync(bytes, 0, bytes.Length);
                    SourceStream.Close();
                }
                WriteMetaFile(path, metaContent);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                paths[i] = path;
            }

            if (type == AssetType.Spritesheet)
            {
                var dict = packs.GroupBy(x => x.Parent).ToDictionary(k => k.Key, v => v.Select(x => x.Name).ToArray());
                SaveSpriteAtlas(dict);
            }
            AssetDatabase.SaveAssets();
        }

        private void WriteMetaFile(string path, string metaContent)
        {
            if (metaContent != string.Empty)
            {
                FileInfo fi = new FileInfo($"{path}.meta");
                using TextWriter txtWriter = new StreamWriter(fi.Open(FileMode.Create));
                txtWriter.Write(metaContent);
            }
        }

        private void SaveSpriteAtlas(Dictionary<string, string[]> textures)
        {
            foreach (var pair in textures)
            {
                var path = $"{TEXTURES_PATH}/{pair.Key}/{pair.Key}.spriteatlas";

                SpriteAtlas sa = new SpriteAtlas();
                sa.SetPackingSettings(new SpriteAtlasPackingSettings() { enableTightPacking = false, enableRotation = false, padding = 4 });

                AssetDatabase.DeleteAsset(path);
                AssetDatabase.CreateAsset(sa, path);
                Sprite[] sprites = new Sprite[pair.Value.Length];

                for(int i = 0; i < pair.Value.Length; ++i)
                {
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURES_PATH}/{pair.Key}/{pair.Value[i]}");
                }

                if (sprites != null)
                    SpriteAtlasExtensions.Add(sa, sprites);
            }
        }
    }
}