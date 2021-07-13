using System;
using UnityEngine;

namespace BundlesLoader.Bundles.Loaders.SpriteRendererLoader
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererRuntimeTextureBundleLoader : RuntimeTextureBundleLoader
    {
        private SpriteRenderer spriteRenderer;

        protected override void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
        }
    }
}