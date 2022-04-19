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

            if (sprite == null || rectTransform == null)
                return;

            if(image.type == Image.Type.Sliced)
            {
                image.pixelsPerUnitMultiplier = Mathf.Clamp(image.sprite.rect.height /
                    Mathf.Clamp(rectTransform.rect.height, 1, rectTransform.rect.height), 0f, 3.5f);
            }
        }
    }
}