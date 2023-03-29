using System.Collections.Generic;
using UnityEngine;

namespace BundlesLoader.Gif
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GifSprite : MonoBehaviour
    {
        private SpriteRenderer img;
        private List<Sprite> frames = new List<Sprite>();
        private List<float> frameDelays = new List<float>();
        private bool isPlaying;
        private bool isOneShot;
        private int index;
        private float timer;

        public bool IsPlaying -> isPlaying;

        private void Awake()
        {
            img = GetComponent<SpriteRenderer>();
        }

        public void Initialize(GifData<Sprite> gifData)
        {
            frames = gifData.Frames;
            frameDelays = gifData.Delays;
            Play();
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