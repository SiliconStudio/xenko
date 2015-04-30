// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// A base class for rendering tests
    /// </summary>
    public class UnitTestGameBase : GraphicsTestBase
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
                Camera.Add(value);
                graphicsCompositor.Cameras[0] = value;
            }
        }

        /// <summary>
        /// Create an instance of the game test
        /// </summary>
        public UnitTestGameBase()
        {
            StopOnFrameCount = -1;

            GraphicsDeviceManager.PreferredGraphicsProfile = new [] { GraphicsProfile.Level_11_0 };

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

            Scene.AddChild(UIRoot);
            Scene.AddChild(Camera);

            Camera.Transform.Position = new Vector3(0, 0, 1000);

            UIComponent.IsFullScreen = true;
            UIComponent.VirtualResolution = new Vector3(1000, 500, 500);
            UIComponent.VirtualResolutionMode = VirtualResolutionMode.FixedWidthFixedHeight;
        }

        protected virtual void SpecificDrawBeforeUI(RenderContext context, RenderFrame renderFrame)
        {
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Set dependency properties test values.
            TextBlock.TextColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.LightGray);
            EditText.TextColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.LightGray);
            EditText.SelectionColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.FromAbgr(0x623574FF));
            EditText.CaretColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.FromAbgr(0xF0F0F0FF));
            var buttonPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonPressed);
            var buttonNotPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonNotPressed);
            var buttonOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonOverred);
            Button.PressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button pressed design", buttonPressedTexture) { Borders = 8 * Vector4.One });
            Button.NotPressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button not pressed design", buttonNotPressedTexture) { Borders = 8 * Vector4.One });
            Button.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button overred design", buttonOverredTexture) { Borders = 8 * Vector4.One });
            var editActiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextActive);
            var editInactiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextInactive);
            var editOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextOverred);
            EditText.ActiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit active design", editActiveTexture) { Borders = 12 * Vector4.One });
            EditText.InactiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit inactive design", editInactiveTexture) { Borders = 12 * Vector4.One });
            EditText.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit overred design", editOverredTexture) { Borders = 12 * Vector4.One });
            var toggleButtonChecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonChecked);
            var toggleButtonUnchecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonUnchecked);
            var toggleButtonIndeterminate = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonIndeterminate);
            ToggleButton.CheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button checked design", toggleButtonChecked) { Borders = 8 * Vector4.One });
            ToggleButton.UncheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button unchecked design", toggleButtonUnchecked) { Borders = 8 * Vector4.One });
            ToggleButton.IndeterminateImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button indeterminate design", toggleButtonIndeterminate) { Borders = 8 * Vector4.One });

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