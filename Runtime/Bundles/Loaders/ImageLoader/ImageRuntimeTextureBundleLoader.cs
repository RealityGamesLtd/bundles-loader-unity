using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders.ImageLoader
{
    [RequireComponent(typeof(Image))]
    public class ImageRuntimeTextureBundleLoader : RuntimeTextureBundleLoader
    {
        private Image image;

        public Sprite sprite
        {
            get
            {
                if (image == null)
                {
                    image = GetComponent<Image>();
                }

                return image.sprite;
            }
            set
            {
                if (image == null)
                {
                    image = GetComponent<Image>();
                }

                image.sprite = value;
            }
        }

        protected override void SetSprite(Sprite sprite)
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            image.sprite = sprite;
        }
    }
}