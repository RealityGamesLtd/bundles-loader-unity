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
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{spriteName}";

            if (!IsValidAssetsService())
            {
                return false;
            }

            var bundle = AssetRetriever.GetBundle(bundleName);
            if (bundle != null)
                bundle.OnAssetsChanged += OnAssetChanged;
            return SetStandaloneTexture(bundleType.FullName.Split('/'), bundle);
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
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{atlasName}/{spriteName}";

            if (!IsValidAssetsService())
            {
                return false;
            }

            var bundle = AssetRetriever.GetBundle(bundleName);

            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle {bundleName}, will not load sprite");
                return false;
            }

            bundle.OnAssetsChanged += OnAssetChanged;
            return SetSpriteAtlasTexture(bundleType.FullName.Split('/'), bundle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns>True if sprite was found and loaded, false otherwise.</returns>
        public bool LoadSprite(AssetType assetType)
        {
            bundleType.FullName = assetType.FullPath;

            if (!IsValidAssetsService())
            {
                return false;
            }

            var bundle = AssetRetriever.GetBundle(assetType);

            if (bundle == null)
            {
                Debug.LogError($"Could not get bundle with path {assetType.FullPath}, will not load sprite");
                return false;
            }

            bundle.OnAssetsChanged += OnAssetChanged;

            var split = bundleType.FullName.Split('/');
            if (split.Length == 4)
            {
                return SetSpriteAtlasTexture(split, bundle);
            }
            else if (split.Length == 3)
            {
                return SetStandaloneTexture(split, bundle);
            }
            else
            {
                Debug.LogError($"Wrong format: {assetType}!");
                return false;
            }
        }

        private bool SetStandaloneTexture(string[] split, Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return false;
            }

            var texture = bundle.LoadAsset<Texture2D>(split[2]);
            var sprite = bundle.LoadAsset<Sprite>(split[2]);
            if (texture == null && sprite == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}");
                LogError(new AssetCallback(AssetErrorType.NULL_TEXTURE, $"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}",
                    $":{split[0]}/{split[1]}", split[2]));
                return false;
            }

            if (texture != null)
                SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            else if (sprite != null)
                SetSprite(sprite);

            return true;
        }

        private bool SetSpriteAtlasTexture(string[] split, Bundle bundle)
        {
            if (bundle == null)
            {
                Debug.LogError($"Could not set sprite atlas texture, bundle provided was NULL");
                return false;
            }

            var atlas = bundle.LoadAsset<SpriteAtlas>(split[2]);
            if (atlas == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITEATLAS, $"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}",
                    $"{split[0]}", split[1]));
                return false;
            }

            var sprite = atlas.GetSprite(split[3]);
            if (sprite == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITE, $"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}",
                    $"{split[0]}/{split[1]}/{split[2]}", split[3]));
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

            var split = bundleType.FullName.Split('/');
            if (split.Length == 4)
            {
                var bundle = AssetRetriever.GetBundle(split[1]);

                if (bundle == null)
                {
                    Debug.LogError($"Could not get bundle with path {split[1]}, will not load sprite");
                    return;
                }

                bundle.OnAssetsChanged -= OnAssetChanged;
            }
            else if (split.Length == 3)
            {
                var bundle = AssetRetriever.GetBundle(split[1]);

                if (bundle == null)
                {
                    Debug.LogError($"Could not get bundle with path {split[1]}, will not load sprite");
                    return;
                }

                bundle.OnAssetsChanged -= OnAssetChanged;
            }
            else
            {
                Debug.LogError($"Wrong format: {bundleType.FullName}!");
            }
        }

        private void OnAssetChanged(Bundle obj)
        {
            if (!IsValidAssetsService())
            {
                return;
            }

            var split = bundleType.FullName.Split('/');
            if (split.Length == 4)
            {
                SetSpriteAtlasTexture(split, obj);
            }
            else if (split.Length == 3)
            {
                SetStandaloneTexture(split, obj);
            }
            else
            {
                Debug.LogError($"Wrong format: {bundleType.FullName}!");
            }
        }
    }
}


