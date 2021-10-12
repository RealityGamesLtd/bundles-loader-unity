using System.Threading.Tasks;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils;
using UnityEditor;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class TexturePack : Pack
    {
        public TexturePack(Texture2D texture, string name, string parent)
        {
            Texture = texture;
            Bytes = texture.EncodeToPNG();
            Name = name;
            Parent = parent;
        }

        public override async Task<AssetPath[]> Save(string texturesPath)
        {
            var paths = await base.Save(texturesPath);

            for(int i = 0; i  < paths.Length; ++i)
            {
                TextureImporter importer = AssetImporter.GetAtPath(paths[i].ToString()) as TextureImporter;
                if (importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }
            return paths;
        }

        public Texture2D Texture { get; private set; }
    }
}
