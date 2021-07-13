using BundlesLoader.Bundles.Loaders.SpriteRendererLoader;
using UnityEditor;

namespace BundlesLoader.CustomInspectors.SpriteRendererLoader
{
    [CustomEditor(typeof(SpriteRendererTextureBundleLoader))]
    public class SpriteRendererTextureBundleLoaderEditor : BaseTextureBundleLoaderEditor<SpriteRendererTextureBundleLoader>
    {
    }
}
