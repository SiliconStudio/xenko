// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// A base class for rendering tests
    /// </summary>
    public class UITestGameBase : GameTestBase
    {
        protected readonly Logger Logger = GlobalLogger.GetLogger("Test Game");
        
        private Vector2 lastTouchPosition;

        protected readonly CameraRendererModeForward SceneCameraRenderer = new CameraRendererModeForward { Name = "Camera Renderers" };

        protected Scene Scene;
        protected Entity Camera = new Entity("Scene camera") { new CameraComponent() };
        protected Entity UIRoot = new Entity("Root entity of camera UI") { new UIComponent()  };

        private readonly SceneGraphicsCompositorLayers graphicsCompositor;

        protected UIComponent UIComponent { get {  return UIRoot.Get<UIComponent>(); } }

        protected CameraComponent CameraComponent
        {
            get { return Camera.Get<CameraComponent>(); }
            set
            {
                bool previousFound = false;
                for (int i = 0; i < Camera.Components.Count; i++)
                {
                    var cameraComponent = Camera.Components[i] as CameraComponent;
                    if (cameraComponent != null)
                    {
                        previousFound = true;
                        if (cameraComponent != value)
                        {
                            if (value == null)
                            {
                                Camera.Components.RemoveAt(i);
                            }
                            else
                            {
                                Camera.Components[i] = value;
                            }
                        }
                        break;
                    }
                }

                if (!previousFound && value != null)
                {
                    Camera.Add(value);
                }

                graphicsCompositor.Cameras[0] = value;
            }
        }

        /// <summary>
        /// Gets the UI system.
        /// </summary>
        /// <value>The UI.</value>
        protected UISystem UI { get; private set; }

        /// <summary>
        /// Create an instance of the game test
        /// </summary>
        public UITestGameBase()
        {
            StopOnFrameCount = -1;

            graphicsCompositor = new SceneGraphicsCompositorLayers
            {
                Cameras = { Camera.Get<CameraComponent>() },
                Master =
                {
                    Renderers =
                    {
                        new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },

                        new SceneDelegateRenderer(SpecificDrawBeforeUI) { Name = "Delegate before main UI" },

                        new SceneCameraRenderer { Mode = SceneCameraRenderer },
                    }
                }
            };
            Scene = new Scene { Settings = { GraphicsCompositor = graphicsCompositor } };

            Scene.Entities.Add(UIRoot);
            Scene.Entities.Add(Camera);

            Camera.Transform.Position = new Vector3(0, 0, 1000);

            UIComponent.IsFullScreen = true;
            UIComponent.Resolution = new Vector3(1000, 500, 500);
            UIComponent.ResolutionStretch = ResolutionStretch.FixedWidthFixedHeight;

            UI = (UISystem)Services.GetService(typeof(UISystem));
            if (UI == null)
            {
                UI = new UISystem(Services);
                GameSystems.Add(UI);
            }
        }

        protected virtual void SpecificDrawBeforeUI(RenderDrawContext context, RenderFrame renderFrame)
        {
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            object existingStyle;

            // Set dependency properties test values.
            UI.DefaultResourceDictionary.TryGetValue(typeof(TextBlock), out existingStyle);
            UI.DefaultResourceDictionary[typeof(TextBlock)] = new Style(typeof(TextBlock), (Style)existingStyle)
            {
                Setters =
                {
                    new Setter<Color>(TextBlock.TextColorPropertyKey, Color.LightGray),
                }
            };

            UI.DefaultResourceDictionary.TryGetValue(typeof(ScrollingText), out existingStyle);
            UI.DefaultResourceDictionary[typeof(ScrollingText)] = new Style(typeof(ScrollingText), (Style)existingStyle)
            {
                Setters =
                {
                    new Setter<Color>(TextBlock.TextColorPropertyKey, Color.LightGray),
                }
            };

            var buttonPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonPressed);
            var buttonNotPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonNotPressed);
            var buttonOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonOverred);
            UI.DefaultResourceDictionary.TryGetValue(typeof(Button), out existingStyle);
            UI.DefaultResourceDictionary[typeof(Button)] = new Style(typeof(Button), (Style)existingStyle)
            {
                Setters =
                {
                    new Setter<ISpriteProvider>(Button.PressedImagePropertyKey, (SpriteFromTexture)new Sprite("Test button pressed design", buttonPressedTexture) { Borders = 8 * Vector4.One }),
                    new Setter<ISpriteProvider>(Button.NotPressedImagePropertyKey, (SpriteFromTexture)new Sprite("Test button not pressed design", buttonNotPressedTexture) { Borders = 8 * Vector4.One }),
                    new Setter<ISpriteProvider>(Button.MouseOverImagePropertyKey, (SpriteFromTexture)new Sprite("Test button overred design", buttonOverredTexture) { Borders = 8 * Vector4.One }),
                }
            };

            var editActiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextActive);
            var editInactiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextInactive);
            var editOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextOverred);
            UI.DefaultResourceDictionary.TryGetValue(typeof(EditText), out existingStyle);
            UI.DefaultResourceDictionary[typeof(EditText)] = new Style(typeof(EditText), (Style)existingStyle)
            {
                Setters =
                {
                    new Setter<Color>(EditText.TextColorPropertyKey, Color.LightGray),
                    new Setter<Color>(EditText.SelectionColorPropertyKey, Color.FromAbgr(0x623574FF)),
                    new Setter<Color>(EditText.CaretColorPropertyKey, Color.FromAbgr(0xF0F0F0FF)),
                    new Setter<ISpriteProvider>(EditText.ActiveImagePropertyKey, (SpriteFromTexture)new Sprite("Test edit active design", editActiveTexture) { Borders = 12 * Vector4.One }),
                    new Setter<ISpriteProvider>(EditText.InactiveImagePropertyKey, (SpriteFromTexture)new Sprite("Test edit inactive design", editInactiveTexture) { Borders = 12 * Vector4.One }),
                    new Setter<ISpriteProvider>(EditText.MouseOverImagePropertyKey, (SpriteFromTexture)new Sprite("Test edit overred design", editOverredTexture) { Borders = 12 * Vector4.One }),
                }
            };

            var toggleButtonChecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonChecked);
            var toggleButtonUnchecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonUnchecked);
            var toggleButtonIndeterminate = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonIndeterminate);
            UI.DefaultResourceDictionary.TryGetValue(typeof(ToggleButton), out existingStyle);
            UI.DefaultResourceDictionary[typeof(ToggleButton)] = new Style(typeof(ToggleButton), (Style)existingStyle)
            {
                Setters =
                {
                    new Setter<ISpriteProvider>(ToggleButton.CheckedImagePropertyKey, (SpriteFromTexture)new Sprite("Test toggle button checked design", toggleButtonChecked) { Borders = 8 * Vector4.One }),
                    new Setter<ISpriteProvider>(ToggleButton.UncheckedImagePropertyKey, (SpriteFromTexture)new Sprite("Test toggle button unchecked design", toggleButtonUnchecked) { Borders = 8 * Vector4.One }),
                    new Setter<ISpriteProvider>(ToggleButton.IndeterminateImagePropertyKey, (SpriteFromTexture)new Sprite("Test toggle button indeterminate design", toggleButtonIndeterminate) { Borders = 8 * Vector4.One }),
                }
            };

            Window.IsMouseVisible = true;

            SceneSystem.SceneInstance = new SceneInstance(Services, Scene);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.FrameCount == StopOnFrameCount || Input.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected PointerEvent CreatePointerEvent(PointerState state, Vector2 position)
        {
            if (state == PointerState.Down)
                lastTouchPosition = position;

            var pointerEvent = new PointerEvent(0, position, position - lastTouchPosition, new TimeSpan(), state, PointerType.Touch, true);

            lastTouchPosition = position;

            return pointerEvent;
        }
    }
}
