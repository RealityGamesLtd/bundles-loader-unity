using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders.ImageLoader
{
    [RequireComponent(typeof(Image))]
    public class ImageRuntimeTextureBundleLoader : RuntimeTextureBundleLoader
    {
        [SerializeField] private bool automaticMultiplier = true;

        private Image image;

        protected override void SetSprite(Sprite sprite)
        {
            base.SetSprite(sprite);
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            image.sprite = sprite;

            if (sprite == null || rectTransform == null)
                return;

            if(image.type == Image.Type.Sliced)
            {
                image.pixelsPerUnitMultiplier = automaticMultiplier ? Mathf.Clamp(image.sprite.rect.height /
                    Mathf.Clamp(rectTransform.rect.height, 1, rectTransform.rect.height), 0f, 3.5f) : 1f;
            }
        }
    }
}