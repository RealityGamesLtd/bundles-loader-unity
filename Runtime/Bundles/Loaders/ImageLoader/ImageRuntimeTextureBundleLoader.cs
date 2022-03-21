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