# IMGUI Translation Loader

Unity's [Immediate Mode GUI](https://docs.unity3d.com/Manual/GUIScriptingGuide.html) is often used to quickly debug Unity games.
However, the IMGUI is also used by many game mods and plug-ins to generate in-game GUIs.

This plug-in allows to easily translate these GUIs.

### [Download the latest release](https://github.com/denikson/IMGUITranslationLoader/releases)
### [View the wiki for help](https://github.com/denikson/IMGUITranslationLoader/wiki)

## Main features

* Based on [YATranslator](https://github.com/denikson/CM3D2.YATranslator)
* Built for Sybaris/ReiPatcher and UnityInjector
* Translates GUIs on by-assembly basis
* Support for RegExes
* Supports dumping original strings

## Motivation

Yes, this plug-in is most likely not the most unique. For instance, I know of CM3D2.UnityUITranslator, but I have yet to locate it.
I already had working core for translation loading, and modifying it to work with IMGUI was simple enough.

I decided against merging this feature into YATranslator simply because of performance factor: YATranslator has many powerful features, but there are simply too many of them to do such a simple task.
Moreover, I'm quite sure not everyone wants to have the all-in-one package like YAT: some may simply want to translate just the IMGUI.

Note that translation IMGUI comes with its own costs: consult the [FAQ](wiki/FAQ) for more info.

## Building

To build you need to:

1. Download the source code
2. Have MSBuild 15.0 installed
3. Place required assemblies into `Libs` folder. More info in the folder's README.
4. Run `build.bat`


## Contributing, problem reporting

If you have suggestions or any problems related to the plug-in, feel free to create an issue.
If you do so, **please**, tag your issues accordingly.

Feel free to fork, edit and create pull requests.
