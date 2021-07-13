using BundlesLoader.Bundles.Loaders.ImageLoader;
using UnityEditor;

namespace BundlesLoader.CustomInspectors.ImageLoader
{
    [CustomEditor(typeof(ImageSpriteAtlasBundleLoader))]
    public class ImageSpriteBundleLoaderEditor : BaseSpriteBundleLoaderEditor<ImageSpriteAtlasBundleLoader>
    {
    }
}
