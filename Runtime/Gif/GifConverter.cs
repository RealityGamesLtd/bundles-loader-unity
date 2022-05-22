using System.Collections.Generic;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;

namespace BundlesLoader.Gif
{
    public class GifData<T>
    {
        public List<T> Frames { get; private set; }
        public List<float> Delays { get; private set; }

        public GifData(List<T> frames, List<float> delays)
        {
            Frames = frames;
            Delays = delays;
        }
    }

    public static class GifConverter
    {
        private static readonly Dictionary<string, GifData<Sprite>> gifSprites = new Dictionary<string, GifData<Sprite>>();
        private static readonly Dictionary<string, GifData<Texture>> gifTextures = new Dictionary<string, GifData<Texture>>();

        public static GifData<Sprite> LoadGifSprite(string name, byte[] bytes, bool forceSwap = false)
        {
            if (!forceSwap && gifSprites.TryGetValue(name, out var gif))
                return gif;

            List<Sprite> frames = new List<Sprite>();
            List<float> frameDelays = new List<float>();
            using var gifStream = new GifStream(bytes);
            while (gifStream.HasMoreData)
            {
                switch (gifStream.CurrentToken)
                {
                    case GifStream.Token.Image:
                        var image = gifStream.ReadImage();
                        var frame = new Texture2D(
                            gifStream.Header.width,
                            gifStream.Header.height,
                            TextureFormat.ARGB32, false);

                        frame.SetPixels32(image.colors);
                        frame.Apply();

                        frames.Add(Sprite.Create(frame,
                            new Rect(0.0f, 0.0f, frame.width, frame.height),
                            new Vector2(0.5f, 0.5f), 100.0f, 0,
                            SpriteMeshType.FullRect));
                        frameDelays.Add(image.SafeDelaySeconds);
                        break;

                    case GifStream.Token.Comment:
                        break;

                    default:
                        gifStream.SkipToken();
                        break;
                }
            }

            gifSprites.Add(name, new GifData<Sprite>(frames, frameDelays));
            return gifSprites[name];
        }

        public static GifData<Texture> LoadGifTexture(string name, byte[] bytes, bool forceSwap = false)
        {
            if (!forceSwap && gifTextures.TryGetValue(name, out var gif))
                return gif;

            List<Texture> frames = new List<Texture>();
            List<float> frameDelays = new List<float>();
            using var gifStream = new GifStream(bytes);
            while (gifStream.HasMoreData)
            {
                switch (gifStream.CurrentToken)
                {
                    case GifStream.Token.Image:
                        var image = gifStream.ReadImage();
                        var frame = new Texture2D(
                            gifStream.Header.width,
                            gifStream.Header.height,
                            TextureFormat.ARGB32, false);

                        frame.SetPixels32(image.colors);
                        frame.Apply();

                        frames.Add(frame);
                        frameDelays.Add(image.SafeDelaySeconds);
                        break;

                    case GifStream.Token.Comment:
                        break;

                    default:
                        gifStream.SkipToken();
                        break;
                }
            }

            gifTextures.Add(name, new GifData<Texture>(frames, frameDelays));
            return gifTextures[name];
        }
    }
}