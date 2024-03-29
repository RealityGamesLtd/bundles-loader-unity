using BundlesLoader.Bundles.Core;
using UnityEngine;
using System;
using UnityEngine.U2D;
using BundlesLoader.Service;
using BundlesLoader.Callbacks;
using Utils;

namespace BundlesLoader.Bundles.Loaders
{
    public abstract class RuntimeTextureBundleLoader : BundleLoader
    {
        protected Sprite sprite;

        /// <summary>
        /// Sets current sprite
        /// </summary>
        /// <param name="sprite">Current sprite</param>
        protected virtual void SetSprite(Sprite sprite)
        {
            this.sprite = sprite;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="spriteName"></param>
        /// <returns>True if sprite was found and loaded, false otherwise.</returns>
        [Obsolete("Use LoadSprite(AssetType) instead")]
        public bool LoadSprite(string bundleName, string spriteName)
        {
            if(sprite != null && bundleType != null)
            {
                //Check if sprite has changed, if not do not GetSprite() from atlas again!
                var hasChanged = spriteName != bundleType.EntityName;
                if (!hasChanged) SetSprite(sprite);
            }

            bundleType.FullName = "TEMP";
            bundleType.RootName = Symbols.BUNDLES_SUBDIRECTORY;
            bundleType.BundleName = bundleName;
            bundleType.EntityName = spriteName;
            bundleType.Type = EntityType.STANDALONE;

            if (!IsValidAssetsService())
            {
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.BundleName}!");
                return false;
            }

            var bundle = AssetRetriever.GetBundle(bundleType.BundleName);
            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle {bundleType.BundleName}, will not load sprite");
                return false;
            }
            bundle.OnAssetsChanged += OnAssetChanged;

            return SetStandaloneTexture(bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="atlasName"></param>
        /// <param name="spriteName"></param>
        /// <returns>True if sprite was found and loaded, false otherwise.</returns>
        [Obsolete("Use LoadSprite(AssetType) instead")]
        public bool LoadSprite(string bundleName, string atlasName, string spriteName)
        {
            if (sprite != null && bundleType != null)
            {
                //Check if sprite has changed, if not do not GetSprite() from atlas again!
                var hasChanged = spriteName != bundleType.EntityName;
                if (!hasChanged) SetSprite(sprite);
            }

            bundleType.FullName = "TEMP";
            bundleType.RootName = Symbols.BUNDLES_SUBDIRECTORY;
            bundleType.BundleName = bundleName;
            bundleType.EntityName = spriteName;
            bundleType.Type = EntityType.ATLAS;

            if (!IsValidAssetsService())
            {
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.BundleName}!");
                return false;
            }

            var bundle = AssetRetriever.GetBundle(bundleType.BundleName);
            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle {bundleType.BundleName}, will not load sprite");
                return false;
            }
            bundle.OnAssetsChanged += OnAssetChanged;

            return SetSpriteAtlasTexture(bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns>True if sprite was found and loaded, false otherwise.</returns>
        public bool LoadSprite(AssetType assetType)
        {
            var parts = assetType.Paths;
            if (sprite != null && bundleType != null)
            {
                //Check if sprite has changed, if not do not GetSprite() from atlas again!
                var hasChanged = parts.AssetName != bundleType.EntityName;
                if (!hasChanged) SetSprite(sprite);
            }

            bundleType.FullName = parts.FullPath;
            bundleType.RootName = parts.RootName;
            bundleType.BundleName = parts.BundleName;
            bundleType.EntityName = parts.AssetName;
            bundleType.Type = parts.Type;

            if (!IsValidAssetsService())
            {
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return false;
            }

            var bundle = AssetRetriever.GetBundle(bundleType.BundleName);
            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle with path {bundleType.BundleName}, will not load sprite");
                return false;
            }

            bundle.OnAssetsChanged += OnAssetChanged;

            if (bundleType.Type == EntityType.ATLAS)
            {
                return SetSpriteAtlasTexture(bundle);
            }
            else if (bundleType.Type == EntityType.STANDALONE)
            {
                return SetStandaloneTexture(bundle);
            }
            else
            {
                Debug.LogError($"Wrong format: {assetType}!");
                return false;
            }
        }

        private bool SetStandaloneTexture(Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up for runtime texture: {bundle.Name}!");
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.EntityName))
            {
                Debug.LogError($"No entity name set up for runtime texture: {bundle.Name}!");
                return false;
            }

            var texture = bundle.LoadAsset<Texture2D>(bundleType.EntityName);
            var sprite = bundle.LoadAsset<Sprite>(bundleType.EntityName);
            if (texture == null && sprite == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no texture:{bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_TEXTURE, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no texture:{bundleType.EntityName}",
                    $":{bundleType.RootName}/{bundleType.BundleName}", bundleType.EntityName));
                return false;
            }

            if (texture != null)
                SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            else if (sprite != null)
                SetSprite(sprite);

            return true;
        }

        private bool SetSpriteAtlasTexture(Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return false;
            }

            var atlas = bundle.LoadAsset<SpriteAtlas>(bundleType.BundleName);
            if (atlas == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no sprite atlas:{bundleType.BundleName}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITEATLAS, $"Bundle:{bundleType.RootName}/{bundleType.BundleName} -> no sprite atlas:{bundleType.BundleName}",
                    $"{bundleType.RootName}", bundleType.BundleName));
                return false;
            }

            if (string.IsNullOrEmpty(bundleType.EntityName))
            {
                Debug.LogError($"No entity name set up: {bundleType.FullName}!");
                return false;
            }

            var sprite = atlas.GetSprite(bundleType.EntityName);
            if (sprite == null)
            {
                Debug.LogError($"Bundle:{bundleType.RootName}/{bundleType.BundleName}, Sprite atlas: {bundleType.BundleName} -> no sprite: {bundleType.EntityName}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITE, $"Bundle:{bundleType.RootName}/{bundleType.BundleName}, " +
                    $"Sprite atlas: {bundleType.BundleName} -> no sprite: {bundleType.EntityName}",
                    $"{bundleType.RootName}/{bundleType.BundleName}/{bundleType.BundleName}", bundleType.EntityName));
                return false;
            }

            SetSprite(sprite);

            return true;
        }

        private void OnDestroy()
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            if (string.IsNullOrEmpty(bundleType.BundleName))
            {
                Debug.LogError($"No bundle name set up: {bundleType.FullName}!");
                return;
            }

            var bundle = AssetRetriever.GetBundle(bundleType.BundleName);
            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle with path {bundleType.BundleName}, will not load sprite");
                return;
            }

            bundle.OnAssetsChanged -= OnAssetChanged;
        }

        private void OnAssetChanged(Bundle obj)
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            if (bundleType.Type == EntityType.ATLAS)
            {
                SetSpriteAtlasTexture(obj);
            }
            else if (bundleType.Type == EntityType.STANDALONE)
            {
                SetStandaloneTexture(obj);
            }
            else
            {
                Debug.LogError($"Wrong format: {bundleType.FullName}!");
            }
        }
    }
}


