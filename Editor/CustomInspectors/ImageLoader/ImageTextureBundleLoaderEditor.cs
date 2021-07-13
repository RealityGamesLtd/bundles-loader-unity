using BundlesLoader.Bundles.Loaders.ImageLoader;
using UnityEditor;

namespace BundlesLoader.CustomInspectors.ImageLoader
{
    [CustomEditor(typeof(ImageTextureBundleLoader))]
    public class ImageTextureBundleLoaderEditor : BaseTextureBundleLoaderEditor<ImageTextureBundleLoader>
    {
    }
}
