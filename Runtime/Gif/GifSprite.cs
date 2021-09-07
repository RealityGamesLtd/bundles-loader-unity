using System.Collections.Generic;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;

namespace BundlesLoader.Gif
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GifSprite : MonoBehaviour
    {
        private SpriteRenderer img;
        private readonly List<Sprite> frames = new List<Sprite>();
        private readonly List<float> frameDelays = new List<float>();
        private bool isPlaying;
        private bool isOneShot;
        private int index;
        private float timer;

        private void Awake()
        {
            img = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (isPlaying)
            {
                if(frames.Count > 0 && frameDelays.Count > 0)
                {
                    var frame = frames[index];
                    var delay = frameDelays[index];
                    timer += Time.deltaTime;
                    if (timer > delay)
                    {
                        img.sprite = frame;
                        index += 1;
                        index %= frames.Count;

                        if(isOneShot && index == frames.Count - 1)
                        {
                            isPlaying = false;
                        }
                        timer = 0f;
                    }
                }
            }
        }

        public void Load(byte[] gif)
        {
            using var gifStream = new GifStream(gif);
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

            Play();
        }

        public void Play()
        {
            gameObject.SetActive(true);
            isPlaying = true;
            isOneShot = false;
        }

        public void Stop()
        {
            gameObject.SetActive(false);
            isPlaying = false;
            timer = index = 0;
        }

        public void PlayOneShot()
        {
            gameObject.SetActive(true);
            isPlaying = true;
            isOneShot = true;
        }

        public void Pause()
        {
            gameObject.SetActive(true);
            isPlaying = false;
        }
    }
}