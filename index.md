---
layout: project
title: "IMGUITranslationLoader"
project_title: "IMGUI Translation Loader"
project_subtitle: "A minimalistic translation loader for Unity IMGUI"
asset_types:
  - postfix: "ReiPatcher"
    index: 0
  - postfix: "Sybaris"
    index: 1
caption_file: "caption.md"
jumbo_content_file: "side.html"
sections:
  - id: "help"
    name: "Help and resources"
    contents_file: "help_section.md"
buttons:
  - name: "Installation guide"
    href: "https://github.com/denikson/IMGUITranslationLoader/wiki/Installation"
  - name: "View Wiki"
    href: "https://github.com/denikson/IMGUITranslationLoader/wiki"
---

IMGUI Translation Loader is a patch/plug-in that allows to replace label texts within Unity's IMGUI.

This translation loader is minimal and game-agnostic. As long as the Unity games supports running .NET or Mono, this plug-in can be used!

With this tool one won't have to modify plug-ins to provide needed localisation! All translations are kept tidy in separate text files that can be easily created, modified and shared.

## Why use IMGUI Translation Loader?

Some games have vast plug-in support. Since IMGUI is the only fully universal GUI provided by Unity, most plug-in developers pick IMGUI for their plug-in UIs. 

Most of the time plug-in developers provide little to no support for translation of the UI. IMGUI Translation Loader aims to address the issue by making it possible to translate *any* IMGUI.

<div class="alert alert-warning" role="alert">
  <h4 class="alert-heading"><i class="fas fa-exclamation-triangle"></i> Note!</h4>
  <p>
      While this tool was made with speed in mind, there are known performance drawbacks.
      Read more about them <a class="alert-link" href="https://github.com/denikson/IMGUITranslationLoader/wiki/Writing-translations#using-regexes-reduces-performance">in the translators' guide</a> and the <a class="alert-link" href="https://github.com/denikson/IMGUITranslationLoader/wiki/FAQ#q-why-is-translation-so-slow">FAQ</a>.
  </p>
</div>


## How it works

IMGUI Translation Loader uses [Cecil](https://github.com/jbevain/cecil) and [Cecil.Inject](https://github.com/denikson/Mono.Cecil.Inject) to patch Unity's IMGUI.

Every time IMGUI is drawn, IMGUI Translation Loader catches any drawn label text and replaces it with user-specified one.
IMGUI supports Regular Expressions, which allows to preserve parts of original labels, like number and variable values.