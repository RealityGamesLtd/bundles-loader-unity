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
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window
{
    public class SpriteDownloaderWindow : EditorWindow
    {
        private enum AssetType
        {
            Standalone,
            Spritesheet
        }

        private const int GRID_COLUMNS = 4;

        private const string PACKAGE_NAME = "com.realitygames.bundlesloader";
        private const string TEXTURES_PATH = "Assets/Textures/spritesheets";

        private readonly UnityEvent OnDownloaded = new UnityEvent();
        private readonly UnityEvent OnSaved = new UnityEvent();
        private readonly string[] SCOPES = { DriveService.Scope.DriveReadonly };

        private AssetType currentType = AssetType.Spritesheet;
        private string driveFolderID;
        private Vector2 scrollPos;
        private List<Pack> processingPacks;
        private List<ConfigPack> configPacks;
        private Dictionary<string, List<Pack>> filteredPacks;
        private DriveService driveService;

        int currentIndex;
        int finalCount = 1;
        bool isProgressing = false;

        private GUIStyle packStyle;

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
            OnSaved.AddListener(async () =>
            {
                currentIndex = 0;
                finalCount = 1;
                await Task.WhenAny(StartProgress("Asset Importer", "Importing assets..."), SaveAssets(currentType));
                Debug.Log("DONE SAVING!");
            });

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
                currentIndex = 0;
                finalCount = 1;
                await Task.WhenAny(StartProgress("Asset Downloader", "Downloading assets..."), DownloadAssets());
                Debug.Log("DONE DOWNLOADING!");
            });

            DrawPreview();
        }

        private void DrawPreview()
        {
            if (filteredPacks != null)
            {
                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(392), GUILayout.Height(295));

                EditorGUILayout.BeginHorizontal();

                var div = (int)Mathf.Floor(filteredPacks.Count / GRID_COLUMNS);
                var rest = filteredPacks.Count % GRID_COLUMNS;
                var cols = (rest > 0 ? GRID_COLUMNS + 1 : GRID_COLUMNS);

                for (int i = 0; i < cols; ++i)
                {
                    DrawVertical(i, div);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawVertical(int i, int v)
        {
            packStyle = new GUIStyle(GUI.skin.box);
            packStyle.normal.background = WindowUtility.MakeTex(72, 72, Color.red);
            EditorGUILayout.BeginVertical();
            for(int j = 0; j < v;  ++j)
            {
                var counter = j + i * v;
                if(counter < filteredPacks.Count)
                {
                    DrawElement(ref counter);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private int DrawElement(ref int counter)
        {
            var elem = filteredPacks.ElementAt(counter++);
            EditorGUILayout.BeginVertical(packStyle, GUILayout.Width(72), GUILayout.Height(72));
            GUILayout.Label(elem.Key, EditorStyles.wordWrappedLabel, GUILayout.Width(65));

            switch (currentType)
            {
                case AssetType.Spritesheet:
                    GUILayout.Box(AssetPreview.GetMiniTypeThumbnail(typeof(SpriteAtlas)), packStyle, GUILayout.Width(65), GUILayout.Height(55));
                    break;
                case AssetType.Standalone:
                    GUILayout.Box(AssetPreview.GetMiniTypeThumbnail(typeof(Texture2D)), packStyle, GUILayout.Width(65), GUILayout.Height(55));
                    break;
            }
            EditorGUILayout.EndVertical();
            return counter;
        }

        private async Task StartProgress(string label, string info)
        {
            isProgressing = true;
            while (isProgressing)
            {
                EditorUtility.DisplayProgressBar(label, info, (float)currentIndex / finalCount);
                await Task.Delay(100);
            }
            EditorUtility.ClearProgressBar();
        }

        private async Task DownloadAssets()
        {
            var parentFolder = await driveService.Files.Get(driveFolderID).ExecuteAsync();
            var folderName = parentFolder.Name;

            var listRequest = driveService.Files.List();
            listRequest.Q = $"'{driveFolderID}' in parents and mimeType = 'application/vnd.google-apps.folder'";
            var res = await listRequest.ExecuteAsync();
            var folders = res.Files.ToArray();

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

            finalCount = tasks.Count;

            List<(byte[] Bytes, string FolderName, string Name)> resp = new List<(byte[] Bytes, string FolderName, string Name)>();

            try
            {
                foreach(var task in tasks)
                {
                    resp.Add(await task);
                    await Task.Yield();
                    currentIndex++;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                processingPacks = null;
            }

            if (resp.Count == 0)
            {
                Debug.LogError("Bytes are null!");
                processingPacks = null;
            }

            var lclPcks = ParsePacks(resp);

            configPacks = lclPcks.Where(x => x is ConfigPack).Select(x => x as ConfigPack).ToList();
            processingPacks = lclPcks.Where(x => !(x is AtlasConfigPack || x is TextureConfigPack)).ToList();
            filteredPacks = processingPacks.GroupBy(x => x.Parent).ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            isProgressing = false;
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

            var ret = files.Select(async x =>
            {
                var task = driveService.Files.Get(x.Id);
                using MemoryStream ms = new MemoryStream();
                await task.DownloadAsync(ms);
                return (ms.GetBuffer(), $"{subFolderName}", $"{x.Name}");
            });
            return ret;
        }

        private List<Pack> ParsePacks(List<(byte[] Bytes, string FolderName, string Name)> resp)
        {
            List<Pack> packs = new List<Pack>();

            switch (currentType)
            {
                case AssetType.Spritesheet:
                    var dict = resp
                    .GroupBy(x => x.FolderName)
                    .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                    foreach (var pair in dict)
                    {
                        List<TexturePack> textures = new List<TexturePack>();
                        pair.Value.ForEach(x =>
                        {
                            var pck = GetPack(x, currentType);
                            if (pck != null && pck is TexturePack texPack)
                                textures.Add(texPack);
                            else if (pck != null && pck is AtlasConfigPack confPack)
                                packs.Add(pck);
                        });

                        if(textures.Count > 0)
                            packs.Add(new AtlasPack(textures, string.Empty, pair.Key));
                    }
                    break;
                case AssetType.Standalone:
                    for (int i = 0; i < resp.Count; ++i)
                    {
                        var pck = GetPack(resp[i], currentType);
                        if(pck != null)
                            packs.Add(pck);
                    }
                    break;
            }
            return packs;
        }

        private Pack GetPack((byte[] Bytes, string FolderName , string Name) resp, AssetType currentType)
        {
            if (Regex.Match(resp.Name, AssetsRegexs.TEXTURE_REGEX).Success)
            {
                var tex = new Texture2D(1, 1);
                tex.LoadImage(resp.Bytes);
                return new TexturePack(tex, resp.Name, resp.FolderName);
            }
            else if (Regex.Match(resp.Name, AssetsRegexs.BYTE_REGEX).Success)
            {
                return new BytePack(resp.Bytes, resp.Name, resp.FolderName);
            }
            else if (Regex.Match(resp.Name, AssetsRegexs.CONFIG_REGEX).Success)
            {
                if (currentType == AssetType.Standalone)
                    return new TextureConfigPack(resp.Bytes, resp.Name, resp.FolderName);
                else if (currentType == AssetType.Spritesheet)
                    return new AtlasConfigPack(resp.Bytes, resp.Name, resp.FolderName);
                else
                    return null;
            }
            else
            {
                //TODO: More regexes in the future
                Debug.LogError($"File: {resp.Name}  inside folder: {resp.FolderName} unsupported format!");
                return null;
            }
        }

        private async Task SaveAssets(AssetType type)
        {
            finalCount = processingPacks.Count;

            if (processingPacks == null)
            {
                EditorUtility.DisplayDialog("Sprite Downloader",
                    "There are no textures to be saved", "Ok");
                Debug.LogError("Textures are null");
                isProgressing = false;
                return;
            }

            if(configPacks == null)
            {
                EditorUtility.DisplayDialog("Sprite Downloader",
                    "There are no textures to be saved", "Ok");
                Debug.LogError("Configs are null, probably wrong mode!");
                isProgressing = false;
                return;
            }

            if (type == AssetType.Spritesheet)
            {
                var atlasPacks = processingPacks.Select(x => x as AtlasPack).ToList();
                var paths = await ImportAssets(atlasPacks);
                SaveSpriteAtlas(atlasPacks.ToDictionary(x => x.Parent, v => v.Textures));
            }
            else
            {
                var paths  = await ImportAssets(processingPacks);
                SaveTexture(paths);
            }
            AssetDatabase.SaveAssets();
            isProgressing = false;
        }

        private async Task<List<AssetPath>> ImportAssets<T>(List<T> texturePacks)
            where T : Pack
        {
            List<AssetPath> paths = new List<AssetPath>();
            for (int i = 0; i < texturePacks.Count; ++i)
            {
                var savedPaths = await texturePacks[i].Save(TEXTURES_PATH);
                currentIndex++;
                paths.AddRange(savedPaths);
            }
            return paths;
        }

        private void SaveTexture(List<AssetPath> paths)
        {
            var configs = configPacks.Select(x => x as TextureConfigPack).ToList();

            for (int i = 0; i < paths.Count; ++i)
            {
                TextureImporter importer = AssetImporter.GetAtPath(paths[i].ToString()) as TextureImporter;
                var configEntry = configs.Find(x => x.Parent.Equals(paths[i].ParentName));
                if (importer != null && configEntry != null && configEntry.AssetConfigs !=  null)
                {
                    if(configEntry.AssetConfigs.TryGetValue(paths[i].AssetName, out var config))
                    {
                        SetUpPlatformSetting(importer, config, BuildTarget.iOS);
                        SetUpPlatformSetting(importer, config, BuildTarget.Android);
                        AssetDatabase.ImportAsset(paths[i].ToString(), ImportAssetOptions.ForceUpdate);                       
                    }
                    else
                    {
                        Debug.LogError($"No compression config for: {paths[i].AssetName}");
                    }
                }
                else
                {
                    Debug.LogError($"No compression config for: {paths[i].ParentName}");
                }
                currentIndex++;
            }
        }

        private void SaveSpriteAtlas(Dictionary<string, List<TexturePack>> textures)
        {
            var configs = configPacks.Select(x => x as AtlasConfigPack).ToList();

            foreach (var pair in textures)
            {
                var path = $"{TEXTURES_PATH}/{pair.Key}/{pair.Key}.spriteatlas";
                AssetDatabase.DeleteAsset(path);

                SpriteAtlas sa = new SpriteAtlas();
                sa.SetPackingSettings(new SpriteAtlasPackingSettings() { enableTightPacking = false, enableRotation = false, padding = 4 });
                var config = configs.Find(x => x.Parent.Equals(pair.Key));
                if(config != null)
                {
                    Sprite[] sprites = pair.Value.Select(x => AssetDatabase.LoadAssetAtPath<Sprite>($"{TEXTURES_PATH}/{pair.Key}/{x.Name}")).ToArray();
                    SetUpPlatformSetting(sa, config.AssetConfig, "iPhone");
                    SetUpPlatformSetting(sa, config.AssetConfig, BuildTarget.Android.ToString());
                    if (sprites != null)
                        sa.Add(sprites);

                    AssetDatabase.CreateAsset(sa, path);
                }
                else
                {
                    Debug.LogError($"No compression config for: {pair.Key}");
                }
                currentIndex++;
            }
        }

        private void SetUpPlatformSetting(TextureImporter importer, Models.AssetConfig config, BuildTarget target)
        {
            var platformSettings = importer.GetPlatformTextureSettings(target.ToString());
            platformSettings.overridden = true;
            platformSettings.maxTextureSize = config.MaxTextureSize;
            platformSettings.format = target == BuildTarget.Android ? config.FormatCompressionConfig.Android : config.FormatCompressionConfig.IOS;
            platformSettings.compressionQuality = config.CompressorQuality;
            platformSettings.crunchedCompression = config.CrunchedCompression;
            platformSettings.textureCompression = config.TextureCompression;
            platformSettings.resizeAlgorithm = config.TextureResizeAlgorithm;
            importer.SetPlatformTextureSettings(platformSettings);
            importer.SaveAndReimport();
        }

        private void SetUpPlatformSetting(SpriteAtlas atl, Models.AssetConfig config, string target)
        {
            var platformSettings = atl.GetPlatformSettings(target);
            platformSettings.overridden = true;
            platformSettings.maxTextureSize = config.MaxTextureSize;
            platformSettings.format = target == "Android" ? config.FormatCompressionConfig.Android : config.FormatCompressionConfig.IOS;
            platformSettings.compressionQuality = config.CompressorQuality;
            platformSettings.crunchedCompression = config.CrunchedCompression;
            platformSettings.textureCompression = config.TextureCompression;
            platformSettings.resizeAlgorithm = config.TextureResizeAlgorithm;
            atl.SetPlatformSettings(platformSettings);
            atl.SetIncludeInBuild(false);
        }
    }
}
