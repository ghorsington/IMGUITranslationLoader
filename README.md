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
That said, translating IMGUI is no easy task: the GUI is being redrawn on every tick, which makes retranslation quite costly.
This plug-in attempts to fix the problem by caching already translated strings and bailing out of the translation as soon as possible.


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
