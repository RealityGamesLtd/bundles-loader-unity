using System;
using System.Collections.Generic;
using System.Linq;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Models;
using BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Server.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Packs
{
    public class TextureConfigPack : ConfigPack
    {
        public Dictionary<string, AssetConfig> AssetConfigs { get; private set; }

        public TextureConfigPack(byte[] bytes, string name, string parent) : base(bytes, name, parent) {}

        public override void ParseConfig(string str)
        {
            Dictionary<string, ServerAssetConfig> confs;
            try
            {
                confs = JsonConvert.DeserializeObject<Dictionary<string, ServerAssetConfig>>(str);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}, {Name}, {Parent}");
                return;
            }

            if (confs != null)
            {
                AssetConfigs = confs.ToDictionary(x=>x.Key, y => new AssetConfig(
                    new FormatCompressionConfig(y.Value.FormatCompressionConfig.Android, y.Value.FormatCompressionConfig.IOS),
                    y.Value.TextureCompression,
                    y.Value.MaxTextureSize,
                    y.Value.CompressorQuality,
                    y.Value.CrunchedCompression,
                    y.Value.TextureResizeAlgorithm));
            }
            else
            {
                Debug.LogError("Config value is null!");
            }
        }
    }
}
