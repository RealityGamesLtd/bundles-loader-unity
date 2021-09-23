# Changelog
All notable changes to this project will be documented in this file.

## [1.2.2] - 2021-09-22
- Added bundle loading callback

## [1.2.1] - 2021-09-19
### Fix
- Added Null checking to code

## [1.2.0] - 2021-09-10
- Added padding setting to spriteatlas in sprite downloader
- Added bundles asset type property to handle them from list instead of manual string
- Added progress action callback to bundles loading
- Added runtime mesh renderer texture loader component

## [1.1.1] - 2021-07-30
### Fix
- fixed `RuntimeTextureBundleLoader` single texture loading due to wrong argument using as texture name to load.

## [1.1.0] - 2021-07-28
- Added gif support, fixed handling different extensions files

## [1.0.3] - 2021-07-23
### Fix
- failing cloud builds due to "'AssetDatabase' does not exist in the current context" compilation error
- bug with loading sprites instead of textures from TextureBundleLoader component
### Added
- Standalone mode which saves sprites without packing them inside Sprite Atlas
- Functionality to restore .meta import options of sprites after their redownloading process

## [1.0.2] - 2021-07-16
### Added
- In edit mode in Unity Editor sprites and textures that have bundle loaders with specified sprite asset from a bundle will set their respective renderers with these sprites to enhance visualization while working with prefabs

## [1.0.1] - 2021-07-08
### Added
- Added bundle loaders for SpriteRenderer component
### Refactored
- moved bundle loading logic to base classes 

## [1.0.0] - 2021-06-23
### Added
- Added bundles loaders
