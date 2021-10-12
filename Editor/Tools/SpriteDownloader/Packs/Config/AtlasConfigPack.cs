using System;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Models;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Server.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class AtlasConfigPack : ConfigPack
    {
        public AssetConfig AssetConfig { get; private set; }

        public AtlasConfigPack(byte[] bytes, string name, string parent) : base(bytes, name, parent) {}

        public override void ParseConfig(string str)
        {
            ServerAssetConfig conf;
            try
            {
                conf = JsonConvert.DeserializeObject<ServerAssetConfig>(str);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return;
            }

            if (conf != null && conf.FormatCompressionConfig != null)
            {
                AssetConfig = new AssetConfig(
                    new FormatCompressionConfig(conf.FormatCompressionConfig.Android, conf.FormatCompressionConfig.IOS),
                    conf.TextureCompression,
                    conf.MaxTextureSize,
                    conf.CompressorQuality,
                    conf.CrunchedCompression,
                    conf.TextureResizeAlgorithm);
            }
            else
            {
                Debug.LogError($"Config value error: {Name}, {Parent}");
            }
        }
    }
}
