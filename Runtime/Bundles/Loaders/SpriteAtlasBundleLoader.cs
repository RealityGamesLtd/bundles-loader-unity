using BundlesLoader.Callbacks;
using BundlesLoader.Service;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    [ExecuteAlways]
    public abstract class SpriteAtlasBundleLoader : BundleLoader, IRefreshable
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
                    Debug.LogError($"Wrong bundle name: {bundleType.FullName}!");
                    return;
                }

                var spriteAtlas = AssetLoader.GetAsset<SpriteAtlas>(
                   bundleType.BundleName,
                   bundleType.BundleName);
                if (spriteAtlas == null)
                {
                    Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no sprite atlas:{bundleType.BundleName}");
                    return;
                }

                if (string.IsNullOrEmpty(bundleType.EntityName))
                {
                    Debug.LogError($"Wrong entity name: {bundleType.FullName}!");
                    return;
                }

                var sprite = spriteAtlas.GetSprite(bundleType.EntityName);
                if (sprite == null)
                {
                    Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName}, Sprite atlas: " +
                        $"{bundleType.BundleName} -> no sprite: {bundleType.EntityName}");
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

            var atlas = bundle.LoadAsset<SpriteAtlas>(bundleType.BundleName);
            if (atlas == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no sprite atlas:{bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITEATLAS, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no sprite atlas:{bundleType.EntityName}",
                    $"{bundleType.RootName}", bundleType.BundleName));
                return;
            }

            var sprite = atlas.GetSprite(bundleType.EntityName);
            if (sprite == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName}, Sprite atlas: {bundleType.BundleName} -> no sprite: {bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITE, $"Bundle:{bundleType.RootName}/{bundleType.BundleName}, " +
                    $"Sprite atlas: {bundleType.BundleName} -> no sprite: {bundleType.EntityName}",
                    $"{bundleType.RootName}/{bundleType.BundleName}/{bundleType.BundleName}", bundleType.EntityName));
                return;
            }

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