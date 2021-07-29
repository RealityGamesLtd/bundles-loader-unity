# Changelog
All notable changes to this project will be documented in this file.

## [1.1.0] - 2020-07-28
- Added gif support, fixed handling different extensions files

## [1.0.3] - 2020-07-23
### Fix
- failing cloud builds due to "'AssetDatabase' does not exist in the current context" compilation error
- bug with loading sprites instead of textures from TextureBundleLoader component
### Added
- Standalone mode which saves sprites without packing them inside Sprite Atlas
- Functionality to restore .meta import options of sprites after their redownloading process

## [1.0.2] - 2020-07-16
### Added
- In edit mode in Unity Editor sprites and textures that have bundle loaders with specified sprite asset from a bundle will set their respective renderers with these sprites to enhance visualization while working with prefabs

## [1.0.1] - 2020-07-08
### Added
- Added bundle loaders for SpriteRenderer component
### Refactored
- moved bundle loading logic to base classes 

## [1.0.0] - 2020-06-23
### Added
- Added bundles loaders
