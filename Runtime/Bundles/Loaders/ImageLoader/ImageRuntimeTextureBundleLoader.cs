using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders.ImageLoader
{
    [RequireComponent(typeof(Image))]
    public class ImageRuntimeTextureBundleLoader : RuntimeTextureBundleLoader
    {
        private Image image;

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