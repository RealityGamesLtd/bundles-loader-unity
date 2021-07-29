using System.Collections.Generic;
using ThreeDISevenZeroR.UnityGifDecoder;
using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Gif
{
    [RequireComponent(typeof(RawImage))]
    public class GifImage : MonoBehaviour
    {
        private RawImage img;
        private readonly List<Texture> frames = new List<Texture>();
        private readonly List<float> frameDelays = new List<float>();
        private bool isPlaying;
        private int index;
        private float timer;

        private void Awake()
        {
            img = GetComponent<RawImage>();    
        }

        private void Update()
        {
            if (isPlaying)
            {
                var frame = frames[index];
                var delay = frameDelays[index];
                timer += Time.deltaTime;
                if(timer > delay)
                {
                    img.texture = frame;
                    index += 1;
                    index %= frames.Count;
                    timer = 0f;
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

                        frames.Add(frame);
                        frameDelays.Add(image.SafeDelaySeconds); // More about SafeDelay below
                        break;

                    case GifStream.Token.Comment:
                        var commentText = gifStream.ReadComment();
                        Debug.Log(commentText);
                        break;

                    default:
                        gifStream.SkipToken(); // Other tokens
                        break;
                }
            }

            Play();
        }

        public void Play()
        {
            gameObject.SetActive(true);
            isPlaying = true;
            timer = index = 0;
        }

        public void Stop()
        {
            gameObject.SetActive(false);
            isPlaying = false;
            timer = index = 0;
        }

        public void Pause()
        {
            gameObject.SetActive(true);
            isPlaying = false;
        }
    }
}