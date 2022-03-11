### General
Tool for holding textures/gifs/spriteatlases inside asset bundles to use them on the fly in the game.
All loaders used as components are currently supporting:
- Textures
- Spriteatlas
- Gifs

### Tutorial
## Open asset builder window
![Builder](Readme/builderWindow.png =200x200)
# Fields
In this window you can see some fields:
- Version File -> specifies min/max game version for which current bundles set is being built
- Selected objects -> here you are putting all your objects inside specified hierarchy:
![Builder](Readme/hierarchy.png =200x200)
Basically we have two options, either we are specifing spriteatlas inside asset bundle (if we want to optimize packing) or bundle 
which contains "raw" objects, as you can see in _assets_bg_ bundle (to specify different compression for individual textures)
- Load button -> loads previously built bundles to create user friendly managment
- Build button -> builds bundles

All bundles are built inside "Assets Bundles" directory, for now building on iOS and Android is supported. Output produces names.json and version.json files which will be described in the next section.
# Names.json
File in which all bundles names (with hierarchy) is contained, this file is used to create strings interface for Loaders components to select specific texture/sprite on UI game objects.
# Version.json
```json
"assets_achievements": {
    "Hash": "cb6acc63852be8b5433b860898c0fcf6",
    "CreatedAt": "2022-03-10T18:00:36.698464+01:00",
    "MinVersion": "3.0.30",
    "MaxVersion": "3.0.34"
  }
  ```
File in which all bundles information from build is contained. It holds information about creation date, hash, and min/max version. Hash is used to use caching functionality for example to not download newest bundle from server if the game already downloaded it.
# Mainfest.json
```json
"3.0.29" : "3.0.24",
"3.0.30" : "3.0.30",
"3.0.31" : "3.0.30"
```
Manifest file is used to point specific game version (on the left side) to specific asset bundle directory present on the server inside _android_ or _ios_ directory (right side). <mark>Please be sure to specify all game versions on the left side and point them to any directory.</mark>. Do not duplicate game versions in this json file.

## Specify version
![Builder](Readme/version.png =200x200)
Concept of version is proposed to create an environment in which we can store different versions of bundles for different versions of game. For example if version 3.0.29 is using old button texture and 3.0.30 isn't using this texture, we simply differentiate asset bundle packs on server. <mark>If you are building bundles for specific version of the game, check if you can start form some starting oldest version and use this version as base "minVersion" value, on the other hand "maxVersion" should also be set with caution in the same way.</mark>
If you want to re-build bundles for the same version as it is in asset server you can simply grab "minVersion" and "maxVersion" values from this directory

## Populate bundles
In order to build bundles there is a need to populate selected object list in editor window. Simply drag and drop objects from your unity project assets and create hierarchy in your way. After that, select "fresh build" toggle and build bundles for Android and iOS (you need to do it separately).

## After build
All bundles products are present inside "AssetBundles" directory in your project. Simply grab all files from Android and iOS directory omitting .manifest and .meta files and upload them to directory on your asset server under correct game version directory. Directories on asset server have naming convention based on "minVersion" present in each "version.json" file.

### Package dependencies:
- Newtonsoft.JSON - https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/
- 3DI70R/Unity-GifDecoder - https://github.com/3DI70R/Unity-GifDecoder.git
