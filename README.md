# ModTek

ModTek is a mod-loader for [HBS's BattleTech PC game](https://harebrained-schemes.com/battletech/). It allows modders to create self-contained mods that do not over-write game files. ModTek is run at game startup and dynamically loads mods that conform to the [mod.json format](https://github.com/BattletechModders/ModTek/wiki/The-mod.json-format). Mod dependencies are resolved and load order enforced without needing to edit the dreaded `VersionManifest.csv`. It also provides for incremental patching of stock game files that are easy to remove, version, and persist through patches.

In version 1.7 HBS introduced an internal mod-loader. The in-game mod-loader shares many similarities to ModTek, but has fewer features and less robust handling. We strongly recommend using a stand-alone copy of ModTek instead of the in-game mod-loader. You'll have a better experience and be more in-line with current community best practices.

# Installing ModTek 2.1.0 or later

:warning: If an existing ModTek installation older than 2.1.0 is installed, backup the .json configuration files if you wish, and then remove the folder at `BATTLETECH\Mods\ModTek\`.

Installation of ModTek is straightforward for windows. You download the `ModTek.zip` file and extract it.

1. Download the latest [release from here](https://github.com/BattletechModders/ModTek/releases).
1. Extract the contents of the zip to `BATTLETECH\` so that the `Mods\` folder in the zip appears as `BATTLETECH\Mods\` and the Doorstop files (winhttp.dll etc..) appear directly under `BATTLETECH\`.

:warning: `BATTLETECH\Mods\` is in game installation folder NOT in `Documents\My Games`

On game startup, ModTek decorates the version number found in the bottom left corner of the main menu with "/W MODTEK". If you don't see this something has gone wrong.

## Linux

The zip contains UnityDoorstop script `run.sh` and libraries to run the game with.

### Wine

Using wine is also supported, make sure to let wine load up `winhttp.dll` by setting override to `native, builtin`.

## macOS

:warning: UnityDoorstop should work on macOS but it wasn't tested yet in combination with ModTek.

The zip contains UnityDoorstop script `run.sh` and libraries to run the game with.

### Obsolete

:warning: Obsolete! These installation instructions are for ModTek 2.0 and older.

1. Use the following directory instead of the BATTLETECH directory: ~/Library/Application\ Support/Steam/steamapps/common/BATTLETECH/BattleTech.app/Contents/Resources/
2. If the Mods directory doesn't exist, create it here: ~/Library/Application\ Support/Steam/steamapps/common/BATTLETECH/BattleTech.app/Contents/Resources/Mods/
3. Move the entire ModTek folder from the release download into the /Mods/ folder created above.
4. You should now have a ~/Library/Application\ Support/Steam/steamapps/common/BATTLETECH/BattleTech.app/Contents/Resources/Mods/ModTek/ folder
5. Run the injector program (ModTekInjector.exe) in that folder. To do this:
   a. Open a Terminal window.
   b. At the command line, type "cd ~/Library/Application\ Support/Steam/steamapps/common/BATTLETECH/BattleTech.app/Contents/Resources/Mods/ModTek" and press Return.
   c. At the command line, type "mono ModTekInjector.exe" then press Return to run the injector.
6. DO NOT move anything from the /Mods/ModTek/ folder, it is self-contained.

# Further Documentation

- [The Drop-Dead-Simple-Guide to Installing BTML & ModTek & ModTek mods](doc/QUICKSTART.md)
- [A Brief Primer on Developing ModTek Mods](doc/PRIMER.md)
- [The mod.json Format](doc/MOD_JSON_FORMAT.md)
- [Writing ModTek JSON mods](doc/MOD_JSON.md)
- [Writing ModTek DLL mods](doc/MOD_DLL.md)
- [Advanced JSON Merging](doc/ADVANCED_JSON_MERGING.md)
- [DebugSettings](doc/CUSTOM_TYPE_DEBUGSETTINGS.md)
- [SVGAssets](doc/CUSTOM_TYPE_SVGASSET.md)
- [Custom Tags and Tagsets](doc/CUSTOM_TYPE_CUSTOMTAGS.md)
- [SoundBanks](doc/CUSTOM_TYPE_SOUNDBANKS.md)
- Custom Video - TBD
- [Dynamic Enums / DataAddendumEntries](doc/DATA_ADDENDUM_ENTRIES.md)
- [Manifest Manipulation](doc/MANIFEST.md)
- [Content Pack Assets](doc/CONTENT_PACK_ASSETS.md)

## Developing ModTek

Information on how to build and release ModTek is documented in [DEVELOPER.md](DEVELOPER.md).

## Enabling or Disabling

ModTek 0.7.6 or higher can be enabled or disabled from within the in-game mods menu. If ModTek is enabled, the  "MODS ENABLED" check box will always be set to enabled. To disable ModTek look through the mod list until you find 'ModTek', and disable that 'mod'. Restart the game, and only the in-game mod-loader will be used. Repeat the process but enable the 'ModTek' mod to re-enable an external ModTek install. 

:warning: You must restart the game to enable or disable an external ModTek!

## What is UnityDoorstop and what those files like winhttp.dll

[Unity DoorStop](https://github.com/NeighTools/UnityDoorstop) provides a set of files that trick Unity into loading ModTek, but without modifying the game files.
The old way was asking the user to run a `ModTekInjector.exe` manually, which modified the game so it loaded ModTek.
In cases of updates, the old way required the user to "verify" the game files and re-install mods, but using UnityDoorstop this is not necessary anymore.

## License

ModTek is provided under the [Unlicense](UNLICENSE), which releases the work into the public domain.
