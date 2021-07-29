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

        public Texture2D Texture { get; private set; }
    }
}
