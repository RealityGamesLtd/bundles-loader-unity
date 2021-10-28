using UnityEngine;

namespace BundlesLoader.Bundles.Loaders.SpriteRendererLoader
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererSpriteAtlasBundleLoader : SpriteAtlasBundleLoader
    {
        private SpriteRenderer spriteRenderer;

        public override void SetSprite(Sprite sprite)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
        }
    }
}