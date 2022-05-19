using System;
using Newtonsoft.Json;
using UnityEditor;

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Server.Models
{
    public class ServerAssetConfig
    {
        [JsonProperty("formatCompression")] public ServerFormatCompressionConfig FormatCompressionConfig { get; private set; }
        [JsonProperty("textureCompression")] public string TextureCompression { get; private set; }
        [JsonProperty("maxTextureSize")] public int MaxTextureSize { get; private set; }
        [JsonProperty("compressorQuality")] public int CompressorQuality { get; private set; }
        [JsonProperty("crunchedCompression")] public bool CrunchedCompression { get; private set; }
        [JsonProperty("textureResizeAlgorithm")] public string TextureResizeAlgorithm { get; private set; }
    }

    public class ServerFormatCompressionConfig
    {
        [JsonProperty("android")] public string Android { get; private set; }
        [JsonProperty("ios")] public string IOS { get; private set; }
    }
}

namespace BundlesLoader.EditorHelpers.Tools.SpriteDownloader.Models
{
    public class AssetConfig
    {
        public AssetConfig(FormatCompressionConfig formatCompressionConfig, string textureCompression, int maxTextureSize,
            int compressorQuality, bool crunchedCompression, string textureResizeAlgorithm)
        {
            FormatCompressionConfig = formatCompressionConfig;
            if(Enum.TryParse<TextureImporterCompression>(textureCompression, out var compr))
                TextureCompression = compr;
            else
                TextureCompression = TextureImporterCompression.Compressed;

            if (Enum.TryParse<TextureResizeAlgorithm>(textureResizeAlgorithm, out var resize))
                TextureResizeAlgorithm = resize;
            else
                TextureResizeAlgorithm = TextureResizeAlgorithm.Mitchell;

            MaxTextureSize = maxTextureSize;
            CompressorQuality = compressorQuality;
            CrunchedCompression = crunchedCompression;
        }

        public FormatCompressionConfig FormatCompressionConfig { get; private set; }
        public TextureImporterCompression TextureCompression { get; private set; }
        public int MaxTextureSize { get; private set; }
        public int CompressorQuality { get; private set; }
        public bool CrunchedCompression { get; private set; }
        public TextureResizeAlgorithm TextureResizeAlgorithm { get; private set; }
    }

    public class FormatCompressionConfig
    {
        private const TextureImporterFormat ANDROID_DEFAULT_FORMAT = TextureImporterFormat.ETC2_RGBA8;
        private const TextureImporterFormat IOS_DEFAULT_FORMAT = TextureImporterFormat.PVRTC_RGB4;

        public FormatCompressionConfig(string android, string iOS)
        {
            if (Enum.TryParse<TextureImporterFormat>(android, out var andCompr))
                Android = andCompr;
            else
                Android = ANDROID_DEFAULT_FORMAT;

            if (Enum.TryParse<TextureImporterFormat>(iOS, out var iosCompr))
                IOS = iosCompr;
            else
                IOS = IOS_DEFAULT_FORMAT;
        }

        public TextureImporterFormat Android { get; private set; }
        public TextureImporterFormat IOS { get; private set; }
    }
}
