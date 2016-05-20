// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Interface of the UI system.
    /// </summary>
    public class UISystem : GameSystemBase
    {

        internal UIBatch Batch { get; private set; }

        internal DepthStencilStateDescription KeepStencilValueState { get; private set; }

        internal DepthStencilStateDescription IncreaseStencilValueState { get; private set; }

        internal DepthStencilStateDescription DecreaseStencilValueState { get; private set; }

        internal ResourceDictionary DefaultResourceDictionary { get; private set; }

        private InputManager input;

        public UISystem(IServiceRegistry registry)
            : base(registry)
        {
            Services.AddService(typeof(UISystem), this);
        }

        public override void Initialize()
        {
            base.Initialize();

            input = Services.GetServiceAs<InputManager>();

            Enabled = true;
            Visible = false;

            if (Game != null) // thumbnail system has no game
            {
                Game.Activated += OnApplicationResumed;
                Game.Deactivated += OnApplicationPaused;
            }
        }

        protected override void Destroy()
        {
            if (Game != null) // thumbnail system has no game
            {
                Game.Activated -= OnApplicationResumed;
                Game.Deactivated -= OnApplicationPaused;
            }

            // ensure that OnApplicationPaused is called before destruction, when Game.Deactivated event is not triggered.
            OnApplicationPaused(this, EventArgs.Empty);

            base.Destroy();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            // create effect and geometric primitives
            Batch = new UIBatch(GraphicsDevice);

            // create depth stencil states
            var depthStencilDescription = new DepthStencilStateDescription(true, true)
                {
                    StencilEnable = true,
                    FrontFace = new DepthStencilStencilOpDescription
                    {
                        StencilDepthBufferFail = StencilOperation.Keep,
                        StencilFail = StencilOperation.Keep,
                        StencilPass = StencilOperation.Keep,
                        StencilFunction = CompareFunction.Equal
                    },
                    BackFace = new DepthStencilStencilOpDescription
                    {
                        StencilDepthBufferFail = StencilOperation.Keep,
                        StencilFail = StencilOperation.Keep,
                        StencilPass = StencilOperation.Keep,
                        StencilFunction = CompareFunction.Equal
                    },
                };
            KeepStencilValueState = depthStencilDescription;

            depthStencilDescription.FrontFace.StencilPass = StencilOperation.Increment;
            depthStencilDescription.BackFace.StencilPass = StencilOperation.Increment;
            IncreaseStencilValueState = depthStencilDescription;

            depthStencilDescription.FrontFace.StencilPass = StencilOperation.Decrement;
            depthStencilDescription.BackFace.StencilPass = StencilOperation.Decrement;
            DecreaseStencilValueState = depthStencilDescription;

            // set the default design of the UI elements.
            var designsTexture = TextureExtensions.FromFileData(GraphicsDevice, DefaultDesigns.Designs);

            DefaultResourceDictionary = new ResourceDictionary
            {
                [typeof(Button)] = new Style(typeof(Button))
                {
                    Setters =
                    {
                        new Setter<ISpriteProvider>(Button.PressedImagePropertyKey, (SpriteFromTexture)new Sprite("Default button pressed design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(71, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(Button.NotPressedImagePropertyKey, (SpriteFromTexture)new Sprite("Default button not pressed design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(3, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(Button.MouseOverImagePropertyKey, (SpriteFromTexture)new Sprite("Default button overred design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(37, 3, 32, 32) }),
                    }
                },

                [typeof(EditText)] = new Style(typeof(EditText))
                {
                    Setters =
                    {
                        new Setter<ISpriteProvider>(EditText.ActiveImagePropertyKey, (SpriteFromTexture)new Sprite("Default edit active design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(105, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(EditText.InactiveImagePropertyKey, (SpriteFromTexture)new Sprite("Default edit inactive design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(139, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(EditText.MouseOverImagePropertyKey, (SpriteFromTexture)new Sprite("Default edit overred design", designsTexture) { Borders = 12 * Vector4.One, Region = new RectangleF(173, 3, 32, 32) }),
                    }
                },

                [typeof(ToggleButton)] = new Style(typeof(ToggleButton))
                {
                    Setters =
                    {
                        new Setter<ISpriteProvider>(ToggleButton.CheckedImagePropertyKey, (SpriteFromTexture)new Sprite("Default toggle button checked design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(71, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(ToggleButton.UncheckedImagePropertyKey, (SpriteFromTexture)new Sprite("Default toggle button unchecked design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(3, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(ToggleButton.IndeterminateImagePropertyKey, (SpriteFromTexture)new Sprite("Default toggle button indeterminate design", designsTexture) { Borders = 8 * Vector4.One, Region = new RectangleF(37, 3, 32, 32) }),
                    }
                },

                [typeof(Slider)] = new Style(typeof(Slider))
                {
                    Setters =
                    {
                        new Setter<ISpriteProvider>(Slider.TrackBackgroundImagePropertyKey, (SpriteFromTexture)new Sprite("Default slider track background design", designsTexture) { Borders = 14 * Vector4.One, Region = new RectangleF(207, 3, 32, 32) }),
                        new Setter<ISpriteProvider>(Slider.TrackForegroundImagePropertyKey, (SpriteFromTexture)new Sprite("Default slider track foreground design", designsTexture) { Borders = 0 * Vector4.One, Region = new RectangleF(3, 37, 32, 32) }),
                        new Setter<ISpriteProvider>(Slider.ThumbImagePropertyKey, (SpriteFromTexture)new Sprite("Default slider thumb design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(37, 37, 16, 32) }),
                        new Setter<ISpriteProvider>(Slider.MouseOverThumbImagePropertyKey, (SpriteFromTexture)new Sprite("Default slider thumb overred design", designsTexture) { Borders = 4 * Vector4.One, Region = new RectangleF(71, 37, 16, 32) }),
                        new Setter<ISpriteProvider>(Slider.TickImagePropertyKey, (SpriteFromTexture)new Sprite("Default slider track foreground design", designsTexture) { Region = new RectangleF(245, 3, 3, 6) }),
                        new Setter<float>(Slider.TickOffsetPropertyKey, 13f),
                        new Setter<Vector2>(Slider.TrackStartingOffsetsrPropertyKey, new Vector2(3)),
                    }
                },
            };
        }

        /// <summary>
        /// The method to call when the application is put on background.
        /// </summary>
        void OnApplicationPaused(object sender, EventArgs e)
        {
            // validate the edit text and close the keyboard, if any edit text is currently active
            var focusedEdit = UIElement.FocusedElement as EditText;
            if (focusedEdit != null)
                focusedEdit.IsSelectionActive = false;
        }

        /// <summary>
        /// The method to call when the application is put on foreground.
        /// </summary>
        void OnApplicationResumed(object sender, EventArgs e)
        {
            // revert the state of the edit text here?
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateKeyEvents();
        }

        private void UpdateKeyEvents()
        {
            if (input == null)
                return;

            foreach (var keyEvent in input.KeyEvents)
            {
                if (UIElement.FocusedElement == null || !UIElement.FocusedElement.IsHierarchyEnabled) return;
                var key = keyEvent.Key;
                if (keyEvent.Type == KeyEventType.Pressed)
                {
                    UIElement.FocusedElement.RaiseKeyPressedEvent(new KeyEventArgs { Key = key, Input = input });
                }
                else
                {
                    UIElement.FocusedElement.RaiseKeyReleasedEvent(new KeyEventArgs { Key = key, Input = input });
                }
            }

            foreach (var key in input.KeyDown)
            {
                if (UIElement.FocusedElement == null || !UIElement.FocusedElement.IsHierarchyEnabled) return;
                UIElement.FocusedElement.RaiseKeyDownEvent(new KeyEventArgs { Key = key, Input = input });
            }
        }
    }
}
