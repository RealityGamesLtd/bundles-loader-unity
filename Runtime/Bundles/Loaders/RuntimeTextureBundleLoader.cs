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
        protected abstract void SetSprite(Sprite sprite);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="spriteName"></param>
        /// <returns>True if sprite was found and loaded, false otherwise.</returns>
        [Obsolete("Use LoadSprite(AssetType) instead")]
        public bool LoadSprite(string bundleName, string spriteName)
        {
            bundleType.FullName = "TEMP";
            bundleType.RootName = Symbols.BUNDLES_SUBDIRECTORY;
            bundleType.BundleName = bundleName;
            bundleType.EntityName = spriteName;

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
            bundleType.FullName = "TEMP";
            bundleType.RootName = Symbols.BUNDLES_SUBDIRECTORY;
            bundleType.BundleName = bundleName;
            bundleType.EntityName = spriteName;

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
            bundleType.FullName = parts.FullPath;
            bundleType.RootName = parts.RootName;
            bundleType.BundleName = parts.BundleName;
            bundleType.EntityName = parts.AssetName;

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

            if (parts.Type == EntityType.ATLAS)
            {
                return SetSpriteAtlasTexture(bundle);
            }
            else if (parts.Type == EntityType.STANDALONE)
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
    }
}


