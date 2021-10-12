using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Window.Utils;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class AtlasPack : Pack
    {
        public AtlasPack(List<TexturePack> textures, string name, string parent)
        {
            Textures = textures;

            List<byte> bytes = new List<byte>();
            foreach(var tex in textures)
            {
                bytes.AddRange(tex.Texture.EncodeToPNG());
                var spl = '|';
                bytes.Add(Convert.ToByte(spl));
            }
            Name = name;
            Parent = parent;
        }

        public override async Task<AssetPath[]> Save(string texturesPath)
        {
            List<AssetPath> paths = new List<AssetPath>();

            foreach(var pack in Textures)
            {
                paths.AddRange(await pack.Save(texturesPath));
            }
            return paths.ToArray();
        }

        public List<TexturePack> Textures { get; private set; }
    }
}
