using System;
using UnityEngine;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders.ImageLoader
{
    [RequireComponent(typeof(Image))]
    public class ImageTextureBundleLoader : TextureBundleLoader
    {
        private Image image;

        public override void SetSprite(Sprite sprite)
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            image.sprite = sprite;
        }
    }
}


