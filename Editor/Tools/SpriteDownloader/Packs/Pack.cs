using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils;
using UnityEditor;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class Pack
    {
        public byte[] Bytes { get; set; }
        public string Name { get; set; }
        public string Parent { get; set; }

        public virtual async Task<AssetPath[]> Save(string texturesPath)
        {
            if (!Directory.Exists($"{texturesPath}/{Parent}"))
            {
                Debug.LogWarning($"{texturesPath}/{Parent} directory doesn't exist! Creating a new one!");
                Directory.CreateDirectory($"{texturesPath}/{Parent}");
            }

            if (Regex.Match(Name, AssetsRegexs.BYTE_REGEX).Success)
            {
                Name = $"{Path.GetFileName(Name)}.bytes";
            }

            if (Regex.Match(Name, AssetsRegexs.CONFIG_REGEX).Success)
            {
                return new AssetPath[] { new AssetPath(string.Empty, string.Empty, string.Empty) };
            }

            var path = $"{texturesPath}/{Parent}/{Name}";
            string metaContent = string.Empty;
            if (File.Exists($"{path}.meta"))
            {
                metaContent = File.ReadAllText($"{path}.meta");
                AssetDatabase.DeleteAsset(path);
            }

            using FileStream SourceStream = File.Open(path, FileMode.Create);
            SourceStream.Seek(0, SeekOrigin.End);
            await SourceStream.WriteAsync(Bytes, 0, Bytes.Length);
            SourceStream.Close();
            WriteMetaFile(path, metaContent);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            return new AssetPath[] { new AssetPath(texturesPath, Parent, Name) };
        }

        protected void WriteMetaFile(string path, string metaContent)
        {
            if (metaContent != string.Empty)
            {
                FileInfo fi = new FileInfo($"{path}.meta");
                using TextWriter txtWriter = new StreamWriter(fi.Open(FileMode.Create));
                txtWriter.Write(metaContent);
            }
        }
    }
}
