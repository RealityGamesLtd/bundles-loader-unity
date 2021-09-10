using UnityEngine;

namespace BundlesLoader.Bundles.Loaders.MeshRendererLoader
{
    [RequireComponent(typeof(MeshRenderer))]
    public class RuntimeMeshRendererTextureBundleLoader : RuntimeTextureBundleLoader
    {
        private MeshRenderer meshRenderer;

        protected override void SetSprite(Sprite sprite)
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer == null) return;

            meshRenderer.material.mainTexture = sprite.texture;
        }
    }
}
