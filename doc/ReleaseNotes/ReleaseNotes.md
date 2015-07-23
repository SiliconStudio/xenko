# Highlights

## Default scene

Creating a new game is now easier, as it comes with a simple scene and graphics pipeline already setup for you!

<img src="http://doc.paradox3d.net/1.2/rn_images/DefaultScene.png" align="center" width="500"/>

## Script reloading and live scripting

**Script reloading**: edit C# scripts and have them reloaded live in Game Studio. Previous public values will be kept as much as possible.

<img src="http://doc.paradox3d.net/1.2/rn_images/ScriptReloading1.png" align="center"/>

<img src="http://doc.paradox3d.net/1.2/rn_images/ScriptReloading2.png" align="center"/>

**Live scripting**: Run your game from GameStudio in live-scripting mode; as soon as you update C# scripts, they will be reloaded inside your current running game (with previous public values kept when possible). This allow almost instant iteration on your gameplay code! Powered by Roslyn.

<img src="http://doc.paradox3d.net/1.2/rn_images/LiveScripting.png" align="center" width="500"/>

## Inline documentation

More detailed documentation has been added to many object properties and settings. It is now easily accessible in a dedicated area just below the property grid, when pointing at an item.

<img src="http://doc.paradox3d.net/1.2/rn_images/UserDoc.png" align="center" width="500"/>

## Material and model highlighting

When you work on the materials in your scene, sometimes you want to be able to see where exactly a material is used on a model. In this release, we added a new material highlight feature, which allows you to easily see the different materials in the viewport and in the property grid by moving the mouse over a selected model. It is also possible to highlight a material from the property grid, to identify which part of the model it covers.

<img src="http://doc.paradox3d.net/1.2/rn_images/Material-highlight0.png" align="center" width="500"/>

<img src="http://doc.paradox3d.net/1.2/rn_images/Material-highlight1.png" align="center" width="500"/>

In a similar way, when browsing your assets, you might want to see visually were it is used in the scene. When you select an asset, be it a model, a material, a texture..., the objects using it will be shortly highlighted.

<img src="http://doc.paradox3d.net/1.2/rn_images/Material-highlight2.png" align="center" width="500"/>

## Windows 10

Windows 10 Universal Apps (Store and Phone) are now supported. Add it to your existing projects by right-clicking on your package in Solution Explorer, select "Update Package" and then "Update Platforms".

# Breaking changes
- Assets: Texture default transparency mode changes from *None* to *Auto*.
- Sprites: The *UIImage* and *Sprite* concepts have been merged together. More precisely, for the assets: the *UIImageGroupAsset* and *SpriteGroupAsset* are now included into a single asset named *SpriteSheetAsset*. For the runtime: *UIImageGroup* and *SpriteGroup* have been merged into the *SpriteSheet* class, and *UIImage* and *Sprite* have been merged into the *Sprite* class.
- Sprites: The way the sprites are sized in the scene have changed. A *PixelsPerUnit* field have been added to the sprites. It dictates the size of the sprite in the scene based on their size in pixels. The sprite *ExtrusionMethod* field have been removed. To reproduce the previous sizing behavior, one can enter the size (height or width depending on *ExtrusionMethod*) of the sprite in the *PixelsPerUnits* field.
- Sprites: Sprites are drawn by default with depth test enabled.
- Scripts: *Script* (and *AsyncScript*) no longer provide a *Start* method. Start is now part of the *StartupScript* class.

# Version 1.2.0-beta

Release date: 2015/07/16

## Enhancements
- Assets: Add *Auto* alpha format for automatic alpha detection in textures.
- Effects: FXAA quality setting can be changed and it can now be properly disabled.
- Studio: Add a menu to fix references when deleting an entity that is referenced by other entities of the scene.
- Studio: When selecting a model, material or texture in the asset view, entities that use it will be highlighted in the scene editor
- Studio: Added a material selection mode. When active, it is possible to highlight different materials of a selected entity with the mouse cursor (in the scene editor and in the property grid), and select them in the asset view by clicking.
- Studio: Added a button in the material properties of an entity to highlight them in the scene editor.
- Studio: The sprite editor has been improved.
- Studio: The 'F' key shortcut to focus on selection now also work when the hierarchy tree has the focus in the scene editor.
- Studio: Moving the mouse forward/backward while LMB+RMB are down moves the camera upward/downward.
- Studio: Setting a diffuse map/specular map on a material will also set the diffuse model/specular model of this material.
- Studio: Keyboard shortcut to switch between transformation gizmos (W, E, R, Space by default)
- Engine: Add the possibility to enable/disable the depth test in sprite component.
- Engine: Add a *PremultiplyAlpha* field to *ComputeColor* to be able to easily set pre-multiplied colors from the editor.

## Issues fixed
- Studio: Fix the animation preview that where not properly updating when the source file was modified.
- Studio: Fix an issue that was preventing selection by mouse click to work in the scene editor under some circumstances.
- Studio: The pitch of the scene editor camera is now clamped between -90° and +90°
- Studio: The rotation of the camera in the skybox preview have been fixed.
- Studio: The material preview now properly displays textures mapped to coordinates other than TexCoord0.
- Engine: The center of sprites is now properly taken in account when rendered by the CameraRenderer.
- Engine: Fixed an issue that prevented frustum culling from working.
- Engine: Fixed a crash when members of scripts could not be serialized. The members are now ignored and a warning is generated. ([#228](https://github.com/SiliconStudio/paradox/issues/228)).
- Engine: Fixed an issue where space in shadow maps would not be correcly allocated.
- Engine: Meshes inside a model with negative scaling did not have their faces inverted.
- Shaders: Ambient occlusion now correctly affects ambient light.
- VisualStudio: The SDK version used by the extension is changed based on the version of the package associated with the current solutions. ([#224](https://github.com/SiliconStudio/paradox/issues/224)).

# Version 1.2.1-beta

Release date: 2015/07/24

## Enhancements
- Android: Add support for x86, x86_64 and arm64-v8a processors.

# Known Issues
- UI: EditText is not implemented on Windows Store and Windows Phone.
- Android: Physics engine is not working properly.
- Samples: Material Sample does not work properly on some mob
- Assets: ModelAsset scaling and orientation works only for .FBX, not other formats supported by Assimp library
- Studio: Scripts are not automatically reloaded by the editor. Closing and re-opening it is needed in order to see new scripts.
- Studio: Renaming default scene won’t properly update reference. Please set again the reference in project properties.
- Studio: DDS images cannot be viewed in the Sprite editor
- Studio: Collections in assets properties cannot be edited nor displayed in multi-selection
- Engine: Shadows are currently not supported on mobile platforms