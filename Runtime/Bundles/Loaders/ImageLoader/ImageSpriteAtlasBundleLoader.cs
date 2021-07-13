using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace BundlesLoader.Bundles.Loaders.ImageLoader
{
    [RequireComponent(typeof(Image))]
    public class ImageSpriteAtlasBundleLoader : SpriteAtlasBundleLoader
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


