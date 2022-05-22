using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Gif
{
    [RequireComponent(typeof(RawImage))]
    public class GifImage : MonoBehaviour
    {
        private RawImage img;
        private List<Texture> frames = new List<Texture>();
        private List<float> frameDelays = new List<float>();
        private bool isPlaying;
        private bool isOneShot;
        private int index;
        private float timer;

        private void Awake()
        {
            img = GetComponent<RawImage>();
        }

        public void Initialize(GifData<Texture> gifData)
        {
            frames = gifData.Frames;
            frameDelays = gifData.Delays;
            Play();
        }

        private void Update()
        {
            if (isPlaying)
            {
                if (frames.Count > 0 && frameDelays.Count > 0)
                {
                    var frame = frames[index];
                    var delay = frameDelays[index];
                    timer += Time.deltaTime;
                    if (timer > delay)
                    {
                        img.texture = frame;
                        index += 1;
                        index %= frames.Count;

                        if (isOneShot && index == frames.Count - 1)
                        {
                            isPlaying = false;
                        }
                        timer = 0f;
                    }
                }
            }
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