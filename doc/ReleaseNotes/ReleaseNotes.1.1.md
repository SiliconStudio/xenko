## Paradox 1.1

Highlights:

- A brand new **scene editor** that is now the central piece of Paradox to assemble your game levels, test the rendering, script your entities.
- **Physically Based Rendering** with Layered Material System
- **Scene rendering compositor**, offering a new way to define precisely how to render scenes in your game, apply post effects...etc.
- Easy-to-use and powerful **post-effects API** coming along many built-in effects (Depth Of Field, Bloom, Lens Flare, Glare, ToneMapping, Vignetting, Film Grain, Antialiasing...)
- New implementation of **Shadow Mapping**, supporting SDSM (Sample Distribution Shadow Maps with adaptive depth splits)
- **Scripting System**, to easily add behavior and data to entities
- In Visual Studio, when you edit .pdxsl shaders, there is now **Error Highlighting** and **F12 (Go to Definition)** to make shader editing as smooth as possible.

### Version 1.1.3-beta

Release date: 2015/06/11

#### New Features
- Platforms: Add support for iOS ARM64 and iOS simulator.
- Shaders: Effects can be compiled remotely by host computer when running game on mobile platforms [For iOS see specific documentation]

#### Enhancements
- Assets: Add the possibility to change the orientation of the plane procedural model.
- Studio: Allow to fetch a referenced entity from the property grid in the scene editor.
- Studio: Entities can be duplicated by dragging a transformation gizmo while maintaining ctrl key down.
- Studio: More gizmos, and more options to manage gizmos display (visibility, size…) in the scene editor.
- Studio: Store some settings in an .user file along with the .pdxpkg file.
- Studio: New materials now have at least a white diffuse color by default.
- Studio: When compiling, switch to the build log tab only if there is an error.

#### Issues fixed
- Studio: Adding a Child scene component was crashing the scene editor.
- Studio: Entities with transparent materials could not be selected.
- Studio: Fixed issues when reimporting and merging assets.
- Engine: Fixed lights not being positioned relative to their parents.
- Engine: Fixed wrong lighting after disabling and reenabling light components.
- Engine: Fixed an exception when an object was not in the culling groups of any lights.
- Engine: Ambient occlusion maps now ignore UV scaling overrides as intended.
- Engine: Models with negative scaling did not have their faces inverted.
- Engine: Fixed an issue where cloning an entity with AnimationComponent would cause crashes.
- Engine: Restored frustum culling.
- Engine: Fixed an issue when rendering shadow maps from a child renderer
- Sample: Fixed Forward Lighting sample.
- Shaders: Directional shadow maps were requiring Shader Model 5.0. ([#222](https://github.com/SiliconStudio/paradox/issues/222)).
- Importers: Unicode characters in model node names are correctly imported.

#### Breaking changes
- Engine: The `CameraComponent` is now using the aspect ratio of current viewport by default. This can be changed with `CameraComponent.UseCustomAspectRatio`.
- Graphics: `GraphicsDevice.BackBuffer` and `Graphics.DepthStencilBuffer` are now returning the current back buffer and depth stencil buffer bound to the `GraphicsDevice`, instead of the BackBuffer/DepthStencilBuffer of the screen (eg. `GraphicsDevice.Presenter.BackBuffer`).
- Platform: If you want to target Windows Store/Phone, you need to upgrade Game.dll project to Profile 151, aka ".NET Portable Subset (.NET Framework 4.5.1, Windows 8.1, Windows Phone 8.1)"

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

### Version 1.1.2-beta

Release date: 2015/05/15

#### Issues fixed
- Import: Fixed import of models with material names containing punctuation.
- Studio: Fixed a potential crash when opening a session.
- Studio: Fixed drag'n'drop from the Asset view.

#### Known Issues
- Platforms: Shaders can’t compile due to lack of a proper workflow on other platforms than Windows Desktop  (this will be fixed soon)
- Platforms: Android and iOS platforms are currently not properly supported (this will be fixed soon).
- Platforms: iOS x64 is not yet supported (this will be added soon)
- Assets: Reimporting a Model asset (i.e. FBX) might have issues when merging materials
- Assets: ModelAsset scaling and orientation works only for .FBX, not other formats supported by Assimp library
- Studio: Scripts are not automatically reloaded by the editor. Closing and re-opening it is needed in order to see new scripts.
- Studio: Renaming default scene won’t properly update reference. Please set again the reference in project properties.
- Studio: DDS images cannot be viewed in the Sprite editor
- Studio: Collections in assets properties cannot be edited nor displayed in multi-selection

### Version 1.1.1-beta

Release date: 2015/05/14

#### Enhancements
- Studio: Scene editor opens in Lighting mode when opening a scene that has some light components.
- Studio: More primitives and new icons in the Material preview.
- Studio: Entities can now be drag/dropped in component and entity properties of the property grid.

#### Issues fixed
- Studio: Allows DX10 device as well to display scene -- note that it won't work for scene containing skybox since it requires compute shader 5.0 for prefiltering, i.e. Material sample ([#212](https://github.com/SiliconStudio/paradox/issues/212))
- Studio: Scene loading was stuck in a deadlock on single-core and dual-core CPUs ([#215](https://github.com/SiliconStudio/paradox/issues/215))
- Studio: Fix issues related to non-english locales (numeric inputs and settings save/load) ([#211](https://github.com/SiliconStudio/paradox/issues/211))
- Studio: In some cases, the materials in the scene editor were not properly refreshed after making a change in the related assets.
- Studio: ".jpeg" is now a valid extension for the texture importer.
- Studio: Putting an empty string in the Source or CharacterSet property of the Sprite Font does not cause errors anymore ([#210](https://github.com/SiliconStudio/paradox/issues/210))

#### Known Issues
- Platforms: Shaders can’t compile due to lack of a proper workflow on other platforms than Windows Desktop  (this will be fixed soon)
- Platforms: Android and iOS platforms are currently not properly supported (this will be fixed soon).
- Platforms: iOS x64 is not yet supported (this will be added soon)
- Assets: Reimporting a Model asset (i.e. FBX) might have issues when merging materials
- Assets: ModelAsset scaling and orientation works only for .FBX, not other formats supported by Assimp library
- Studio: Scripts are not automatically reloaded by the editor. Closing and re-opening it is needed in order to see new scripts.
- Studio: Renaming default scene won’t properly update reference. Please set again the reference in project properties.
- Studio: DDS images cannot be viewed in the Sprite editor
- Studio: Collections in assets properties cannot be edited nor displayed in multi-selection

### Version 1.1.0-beta

Release date: 2015/04/28

#### New Features
- Launcher: New **launcher** can now manage several versions of the Paradox SDK
- Studio: Introducing a brand new **scene editor**
- Studio: The scene editor is now the central component of the Studio
- Studio: The asset log panel now display logs (errors, etc.) of the seleced assets and their dependencies
- Studio: Packages now have properties that can be displayed and edited (to set the default scene and some graphics settings)
- Studio: Editor and asset compiler are now **x64** compatible.
- Effects: New built-in **post-effects**: depth-of-field, color aberration, light streaks, lens flares, vignetting, film grain (noise)
- Engine: New Material System supporting **PBR materials**, multi-layered materials with multiple attributes, including: Tessellation, Displacement, Normal, Diffuse, Specular/Metalness, Transparent, Occlusion/Cavity
- Engine: New **rendering pipeline compositor** allowing to compose the rendering of the scene by layers and renderers
- Engine: New Ambient and Skybox lighting
- Engine: New light culling and object culling
- Engine: New implementation of **Shadow Mapping** with support for SDSM (Sample Distribution Shadow Maps with adaptive depth splits)
- Engine: New **scripting system**, to easily add behavior and data to entities.
- Engine: New `ComputeEffectShader` class for compute-shader live compilation and dispatching
- Engine: New entity background component to add a background in a scene
- Engine: New entity UI component to add an UI on entities of the scene.
- Graphics: Add a shared 2x2 pixel white texture on the `GraphicsDevice`  (extension method)
- Input: Add the possibility to hide and lock the mouse using `LockMousePosition` function
- Mathematics: New `SphericalHarmonics` class
- Physics: Renamed PhysicsEngine into Simulation, the engine now supports one separate Simulation for each scene.
- Assets: New asset type `RenderFrameAsset`
- Assets: New asset type `ProceduralModelAsset`

#### Enhancements
- Assets: Yaml now uses a shorter qualified name without culture and keytoken.
- Assets: Added AssetManager.Reload(), to reload on top of existing object
- Assets: During asset compilation, improved logging to always display what asset caused an error
- Assets: Assets can now have “compile-time dependencies” (i.e. when a Material layer embeds/uses another material at compile-time)
- Assets: Add non-generic versions of Load and LoadAsync methods in the AssetManager
- Assets: A Get method that allows to retrieve an already loaded asset without increasing the reference counter
- Assets: An Unload method overload that takes an url as parameter instead of a reference.
- Assets: Asset merging (reimport) is now more flexible
- Assets: Add support for sRGB textures
- Assets: Add support for HDR textures
- Assets/FBX: Add better support for FBX scene unit and up-axis
- Assets/FBX: Automatically generates normal maps if they are not present in a 3d model
- Assets/FBX: Do not merge anymore vertices belonging to different smoothing groups
- Build: Roslyn is now used to compile serialization code (instead of CodeDom)
- Build: Improved logging of asset build
- Build: Parallelization of the build has been improved.
- Core: “Data” classes don’t exist anymore. Now uses AttachedReferenceManager to directly represent design-time and runtime representation of an object with a single unified runtime class.
- Core: Add Collections.PoolListStruct
- Studio: If an asset or one of its dependency has a compile error, properly add a failure sticker on top of thumbnail, and details in the asset log
- Studio: Inside a scene, entities, components and scripts can reference each others.
- Studio: If a script can’t properly be loaded (i.e. due to missing types), be nice and try to keep data as is for next save.
- Studio: Reduce number of threads by sharing build system for assets, scene, preview & thumbnails (with priority management)
- Studio: Shaders are compiled asynchronously (glowing green effect) and compilation errors will be visible (glowing red effect); various shaders are precompiled for faster startup.
- Studio: Improved performance by using binary cloning instead of YAML.
- Studio: Many visual improvement of the Studio user interface
- Graphics: Add GraphicsDevice.PushState/PopState to save/restore Blend, Rasterizer, Depth states and RenderTargets
- Graphics: Add Rasterizer wireframes states
- Graphics: Add support for using new UserDefinedAnnotation for Direct3D11 API profiling
- Graphics: Add support to generate additional texcoords from an existing vertex buffer in VertexHelper
- Graphics: Add possibility to add a back face to the plane geometric primitive
- Graphics: Add the possibility to bind TextureCube and Texture3D to the `SpriteBatch`
- Effects: Infrastructure for recording shader compilations in a Yaml asset and regenerate shaders on different platforms (no UI yet)
- Engine: The local transformation of entity linked with a `ModelNodeLinkComponent` is now taken in account in final world matrix calculation
- Engine: Added `ScriptComponent` to easily add behavior and data to entities directly inside Paradox Studio
- Engine: Materials are defined on Model, but can be overridden in `ModelComponent`
- Engine: Add access to the `SpriteAnimationSystem` from the script context.
- Mathematics: Add `MathUtil.NextPowerOfTwo`
- Mathematics: Add `Vector3` operators with floats
- Mathematics: Add `BoundingSphere.FromPoints` from an native buffer with custom vertex stride
- Mathematics: Add `BoundingBoxExt` for intersection between frustum and bounding box
- Mathematics: Move all `GuillotinePacker` implementation copies to a single implem in Mathematics
- Mathematics: Fix `Color3` to `Color4` implicit operator
- Mathematics: Add `Color3.ToLinear`, `Color3.ToSRgb`, `Color4.ToLinear`, `Color4.ToSRgb` methods
- Mathematics: Add swizzle extension methods for vector classes
- Mathematics: Add explicit conversion method from `Int3` to `Vector3`
- Mathematics: Add `MathUtil.Log2` method
- Mathematics: Add extension method `WithAlpha` to `Color` class. It creates a transparent color from an opaque one
- Physics: Added scaling parameter for Convex Hull Shape asset.
- Physics: Added LocalRotation in collider shape asset description.
- Physics: Removed Sprite workaround, added better default values to shapes.
- VisualStudio: Improve highligting and navigation for `pdxsl` files
- VisualStudio: Improve error messages when `.cs` file generation fails

#### Issues fixed
- Assets: On OpenGL ES 3.0+ targets, HDR textures were converted to LDR during asset compilation.
- Assets/FBX: Animation containing data for only some of the component the Translation/Rotation/Scale are now correctly imported
- Engine: Correctly initialize transformation component rotation to the identity quaternion
- Engine: `EntityManager.Remove` was destroying the hierarchy of the entity
- Input: Fix the key down status when the game lose and gain focus under Windows
- Input: Correctly translate control/shift/alt keys
- Graphics: Implemented `BlendStateDescription.Equals()` and make a readonly copy of `BlendState.Description.RenderTargets` (so that user can't modify it). Fixes #139
- Graphics: Various improvements and bugfixes to OpenGL renderer
- Graphics: Add safeguard to avoid engine crashing when generating extremely big font characters
- Graphics: Fix discontinuity problems in geometric primitive UV coordinates
- Graphics: Fix crash when creating an unordered  texture array of size greater than one
- Graphics: Fix the calculation of `Buffer`’s element count on DirectX
- Mathematics: Matrix.Decompose output now a correct rotation matrix free of reflection
- Mathematics: Fix bug in Matrix.Invert when determinant was too small
- Mathematics: Fix in `Color3.ToRgb` method
- Mathematics: Fix bug in Gradian property of Angle class
- Physics: Fixed PhysicsDebugEffect shader, debug shapes should now render again.
- Physics: Fixed issues related to creating collider shape assets in the Game Studio.
- Shaders: Add missing `GroupMemoryBarrierWithGroupSync` keyword to shading language
- Shaders: Fix order declaration of the constants and  structures in generated shader
- Shaders: Remove generation of key for shader parameters with the `groupshared` keyword
- Studio: Many fixes on the undo/redo stack
- Studio: The build log panel gets the focus only once per build
- Studio: Fix a crash when undocking the Asset log
- Studio: The Studio now have a minimum size at startup
- Studio: Some entries in the settings menu were not working
- Studio: Fix the sound preview when the source file of the asset has been changed

#### Breaking changes
- Android: Android projects should be compiled against Android API v5.0 (only a compile-time requirement, runtime requirement is still Android 2.3+)
- Assets: The entity asset has been removed, entities should be created inside a scene.
- General: Previous Paradox 1.0.x projects cannot be loaded in this new version
- Engine: Deferred lighting was removed. We will later add support for Forward+ and Deferred GBuffer shading
- Engine: `ScriptSystem.Add` has been renamed `ScriptSystem.AddTask`. `Add` is now used only to add scripts
- Engine: Sprites of `SpriteComponents` are now rendered in 3D. Their size is defined by the scale of the entity
- Engine: UI should be configured via entities and `UIComponents` and not via the `UISystem` anymore
- Engine: `VirtualResolution` has been removed from `Game` and should now be set directly in the `UIComponent`
- Engine: Direction of Oz axis have been inversed in the UI to have an RH space
- Graphics: ParameterCollection are now grouped together in a ParameterCollectionGroup at creation time. This object can then be used in Effect.Apply().
- Physics: Collider Shape Asset and Physics Component have been simplified, their asset version is now not compatible with the old version.
- Physics: Debug shape rendering has been replaced by editor gizmos.
- Shaders: Previous generated code for `pdxfx` is broken and must be regenerated
- Studio: Changed naming conventions of imported assets.
- Studio: The studio and asset compilation process are now running only on 64bits machines

#### Known Issues
- Platforms: Shaders can’t compile due to lack of a proper workflow on other platforms than Windows Desktop  (this will be fixed soon)
- Platforms: Android and iOS platforms are currently not properly supported (this will be fixed soon).
- Platforms: iOS x64 is not yet supported (this will be added soon)
- Assets: Reimporting a Model asset (i.e. FBX) might have issues when merging materials
- Assets: ModelAsset scaling and orientation works only for .FBX, not other formats supported by Assimp library
- Studio: Scripts are not automatically reloaded by the editor. Closing and re-opening it is needed in order to see new scripts.
- Studio: Renaming default scene won’t properly update reference. Please set again the reference in project properties.
- Studio: DDS images cannot be viewed in the Sprite editor
- Studio: Collections in assets properties cannot be edited nor displayed in multi-selection
