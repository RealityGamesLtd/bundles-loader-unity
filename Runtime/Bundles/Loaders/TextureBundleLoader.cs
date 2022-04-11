using BundlesLoader.Callbacks;
using BundlesLoader.Service;
using UnityEngine;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class TextureBundleLoader : BundleLoader, IRefreshable
    {
        public abstract void SetSprite(Sprite sprite);

        protected void Awake()
        {
            if (Application.isEditor)
            {
                if (string.IsNullOrEmpty(bundleType.FullName))
                {
                    Debug.LogError("Bundle type not loaded!");
                    return;
                }

                if (string.IsNullOrEmpty(bundleType.BundleName))
                {
                    Debug.LogError($"Wrong format: {bundleType.FullName} !");
                    return;
                }

                var sprite = AssetLoader.GetAsset<Sprite>(
                    bundleType.BundleName,
                    bundleType.EntityName);

                if (sprite == null)
                {
                    Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no texture:{bundleType.EntityName}");
                    return;
                }

                SetSprite(sprite);
            }
            else Initialize();
        }

        private void Initialize()
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            var assetsService = AssetsServiceLoader.AssetsService;
            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up for: {bundleType.FullName} !");
                return;
            }

            if (assetsService.Bundles.TryGetValue(bundleType.BundleName, out var bundle))
            {
                bundle.OnAssetsChanged += OnAssetsChanged;
                SetCurrentAsset(bundle);
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleType.BundleName}");
            }
        }

        private void OnDestroy()
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            var assetsService = AssetsServiceLoader.AssetsService;
            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up for: {bundleType.FullName} !");
                return;
            }

            if (assetsService.Bundles.TryGetValue(bundleType.BundleName, out var bundle))
            {
                bundle.OnAssetsChanged -= OnAssetsChanged;
            }
            else
            {
                Debug.LogError($"No bundle with name: {bundleType.BundleName}");
            }
        }

        public void SetCurrentAsset(Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return;
            }

            if (string.IsNullOrEmpty(bundleType.EntityName))
            {
                Debug.LogError($"No entity name set up: {bundleType.FullName}!");
                return;
            }

            var texture = bundle.LoadAsset<Texture2D>(bundleType.EntityName);
            var sprite = bundle.LoadAsset<Sprite>(bundleType.EntityName);
            if (texture == null && sprite == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no texture:{bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_TEXTURE, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no texture:{bundleType.EntityName}",
                    $":{bundleType.RootName}/{bundleType.BundleName}", bundleType.EntityName));
                return;
            }

            if (texture != null)
                SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            else if (sprite != null)
                SetSprite(sprite);
        }

        public void OnAssetsChanged(Bundle currentBundle)
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up for: {bundleType.FullName} !");
                return;
            }

            SetCurrentAsset(currentBundle);
        }
    }
}


