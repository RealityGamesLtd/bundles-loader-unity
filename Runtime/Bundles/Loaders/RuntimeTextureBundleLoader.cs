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

        [Obsolete("Use LoadSprite(AssetType) instead")]
        public void LoadSprite(string bundleName, string spriteName)
        {
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{spriteName}";

            if (!IsValidAssetsService())
            {
                return;
            }

            var bundle = AssetRetriever.GetBundle(bundleName);
            if (bundle != null)
                bundle.OnAssetsChanged += OnAssetChanged;
            SetStandaloneTexture(bundleType.FullName.Split('/'), bundle);
        }

        [Obsolete("Use LoadSprite(AssetType) instead")]
        public void LoadSprite(string bundleName, string atlasName, string spriteName)
        {
            bundleType.FullName = $"{Symbols.BUNDLES_SUBDIRECTORY}/{bundleName}/{atlasName}/{spriteName}";

            if (!IsValidAssetsService())
            {
                return;
            }

            var bundle = AssetRetriever.GetBundle(bundleName);
            if (bundle != null)
                bundle.OnAssetsChanged += OnAssetChanged;
            SetSpriteAtlasTexture(bundleType.FullName.Split('/'), bundle);
        }

        public void LoadSprite(AssetType assetType)
        {
            bundleType.FullName = assetType.FullName;

            if (!IsValidAssetsService())
            {
                return;
            }

            var bundle = AssetRetriever.GetBundle(assetType);
            if(bundle != null)
                bundle.OnAssetsChanged += OnAssetChanged;

            var split = bundleType.FullName.Split('/');
            if (split.Length == 4)
            {
                SetSpriteAtlasTexture(split, bundle);
            }
            else if(split.Length == 3)
            {
                SetStandaloneTexture(split, bundle);
            }
            else
            {
                Debug.LogError($"Wrong format: {assetType}!");
                return;
            }
        }

        private void SetStandaloneTexture(string[] split, Bundle bundle)
        {
            var texture = bundle.LoadAsset<Texture2D>(split[2]);
            var sprite = bundle.LoadAsset<Sprite>(split[2]);
            if (texture == null && sprite == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}");
                LogError(new AssetCallback(AssetErrorType.NULL_TEXTURE, $"Bundle:{split[0]}/{split[1]} -> no texture:{split[2]}",
                    $":{split[0]}/{split[1]}", split[2]));
                return;
            }

            if (texture != null)
                SetSprite(Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f));
            else if (sprite != null)
                SetSprite(sprite);
        }

        private void SetSpriteAtlasTexture(string[] split, Bundle bundle)
        {
            var atlas = bundle.LoadAsset<SpriteAtlas>(split[2]);
            if (atlas == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITEATLAS, $"Bundle:{split[0]}/{split[1]} -> no sprite atlas:{split[2]}",
                    $"{split[0]}", split[1]));
                return;
            }

            var sprite = atlas.GetSprite(split[3]);
            if (sprite == null)
            {
                Debug.LogError($"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}");
                LogError(new AssetCallback(AssetErrorType.NULL_SPRITE, $"Bundle:{split[0]}/{split[1]}, Sprite atlas: {split[2]} -> no sprite: {split[3]}",
                    $"{split[0]}/{split[1]}/{split[2]}", split[3]));
                return;
            }

            SetSprite(sprite);
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
                if(bundle != null)
                    bundle.OnAssetsChanged -= OnAssetChanged;
            }
            else if (split.Length == 3)
            {
                var bundle = AssetRetriever.GetBundle(split[1]);
                if (bundle != null)
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


