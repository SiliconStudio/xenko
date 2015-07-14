## Paradox 1.2

Highlights:
- **Windows 10** Universal Apps (Store and Phone) are now supported.
- **Script reloading**: edit C# scripts and have them reloaded live in Game Studio. Previous public values will be kept as much as possible.
- **Live scripting**: Run your game from GameStudio in live-scripting mode; as soon as you update C# scripts, they will be reloaded inside your current running game (with previous public values kept when possible). This allow almost instant iteration on your gameplay code! Powered by Roslyn.
- **Default scene**: Creating a new game is now easier than ever, as it comes with a simple scene and graphics pipeline already setup for you!
- **Model and material highlighting**: When selecting a model, material or texture, it will be highlighted in the scene. You can also mouse pick a specific material within a model by using the "Material selection mode".
- Many **Usability** fixes for scene editor, sprite editor, UX, etc...

### Version 1.2.0-beta

Release date: 2015/07/XX

#### New Features

#### Enhancements
- Assets: Add `Auto` alpha format for automatic alpha detection in textures.
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
- Engine: Add a `PremultiplyAlpha` field to `ComputeColor` to be able to easily set pre-multiplied colors from the editor.

#### Issues fixed
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

#### Breaking changes
- Assets: Texture default transparency mode changes from `None` to `Auto`.
- Assets: The `SpriteGroupAsset` and the `UIImageGroupAsset` have been merged into one single `SpriteSheetAsset`. 
- Engine: The `SpriteGroup` and `UIImageGroup` classes have been merged into one single `SpriteSheet` class.
- Engine: The `Sprite` and `UIImage` classes have been merged into one single `Sprite` class.
- Engine: The sprite `ExtrusionMethod` field have been removed and replaced by a `PixelsPerUnit` field in the sprites.
- Engine: Sprites are drawn by default with depth test enabled.

#### Known Issues
- UI: EditText is not implemented on Windows Store and Windows Phone.
- Android: Physics engine is not working properly.
- Samples: Material Sample does not work properly on some mob
- Assets: ModelAsset scaling and orientation works only for .FBX, not other formats supported by Assimp library
- Studio: Scripts are not automatically reloaded by the editor. Closing and re-opening it is needed in order to see new scripts.
- Studio: Renaming default scene won’t properly update reference. Please set again the reference in project properties.
- Studio: DDS images cannot be viewed in the Sprite editor
- Studio: Collections in assets properties cannot be edited nor displayed in multi-selection
- Engine: Shadows are currently not supported on mobile platforms