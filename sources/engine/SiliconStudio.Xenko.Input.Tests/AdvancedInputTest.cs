// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Input.Tests
{
    class AdvancedInputTest : GameTestBase
    {
        private SpriteBatch spriteBatch;

        private SpriteFont spriteFont11;

        private Texture roundTexture;
        private Vector2 roundTextureSize;

        private readonly Color fontColor;

        private const float TextSpaceY = 3;
        private const float TextSubSectionOffsetX = 15;
        private const string KeyboardSessionString = "Keyboard :";

        private float textHeight;
        private readonly Vector2 textLeftTopCorner = new Vector2(5, 5);

        private Vector2 screenSize;

        // keyboard
        private string keyPressed;
        private string keyDown;
        private string keyReleased;

        // mouse
        private Vector2 mousePosition;
        private string mouseButtonPressed;
        private string mouseButtonDown;
        private string mouseButtonReleased;
        private string mouseWheelDelta;

        private readonly Color mouseColor;

        // pointers
        private readonly Queue<Tuple<Vector2, TimeSpan, int>> pointerPressed = new Queue<Tuple<Vector2, TimeSpan, int>>();
        private readonly Queue<Tuple<Vector2, TimeSpan, int>> pointerMoved = new Queue<Tuple<Vector2, TimeSpan, int>>();
        private readonly Queue<Tuple<Vector2, TimeSpan, int>> pointerReleased = new Queue<Tuple<Vector2, TimeSpan, int>>();

        private readonly TimeSpan displayPointerDuration;

        // Gestures
        private string dragEvent;
        private string flickEvent;
        private string longPressEvent;
        private string compositeEvent;
        private string tapEvent;

        private Tuple<GestureEvent, TimeSpan> lastFlickEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<GestureEvent, TimeSpan> lastLongPressEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<GestureEvent, TimeSpan> lastTapEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);

        private readonly TimeSpan displayGestureDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Input"/> class.
        /// </summary>
        public AdvancedInputTest()
        {
            CurrentVersion = 1;

            // create and set the Graphic Device to the service register of the parent Game class
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.PreferredDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
            GraphicsDeviceManager.DeviceCreationFlags = DeviceCreationFlags.None;
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };

            fontColor = Color.Black;
            mouseColor = Color.Gray;

            displayPointerDuration = TimeSpan.FromSeconds(1.5f);
            displayGestureDuration = TimeSpan.FromSeconds(1f);
        }

        protected override Task LoadContent()
        {
            // Load the fonts
            spriteFont11 = Content.Load<SpriteFont>("Arial");

            // load the round texture 
            roundTexture = Content.Load<Texture>("round");

            // create the SpriteBatch used to render them
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // initialize parameters
            textHeight = spriteFont11.MeasureString(KeyboardSessionString).Y;
            screenSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            roundTextureSize = new Vector2(roundTexture.Width, roundTexture.Height);

            // activate the gesture recognitions
            Input.ActivatedGestures.Add(new GestureConfigDrag());
            Input.ActivatedGestures.Add(new GestureConfigFlick());
            Input.ActivatedGestures.Add(new GestureConfigLongPress());
            Input.ActivatedGestures.Add(new GestureConfigComposite());
            Input.ActivatedGestures.Add(new GestureConfigTap());

            // add a task to the task scheduler that will be executed asynchronously 
            Script.AddTask(UpdateInputStates);

            return Task.FromResult(0);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // clear the screen
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.White);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            spriteBatch.Begin(GraphicsContext);

            // render the keyboard key states
            spriteBatch.DrawString(spriteFont11, KeyboardSessionString, textLeftTopCorner, fontColor);
            spriteBatch.DrawString(spriteFont11, "Key pressed: " + keyPressed, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 1 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Key down: " + keyDown, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 2 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Key released: " + keyReleased, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 3 * (textHeight + TextSpaceY)), fontColor);

            // render the mouse key states
            spriteBatch.DrawString(spriteFont11, "Mouse :", textLeftTopCorner + new Vector2(0, 4 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse position: " + mousePosition, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 5 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button pressed: " + mouseButtonPressed, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 6 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button down: " + mouseButtonDown, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 7 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button released: " + mouseButtonReleased, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 8 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse wheel delta: " + mouseWheelDelta, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 9 * (textHeight + TextSpaceY)), fontColor);

            var mouseScreenPosition = new Vector2(mousePosition.X * screenSize.X, mousePosition.Y * screenSize.Y);
            spriteBatch.Draw(roundTexture, mouseScreenPosition, mouseColor, 0, roundTextureSize / 2, 0.1f);

            // render the pointer states
            foreach (var tuple in pointerPressed)
                DrawPointers(tuple, 1.5f, Color.Blue);
            foreach (var tuple in pointerMoved)
                DrawPointers(tuple, 1f, Color.Green);
            foreach (var tuple in pointerReleased)
                DrawPointers(tuple, 2f, Color.Red);

            // render the gesture states
            spriteBatch.DrawString(spriteFont11, "Gestures :", textLeftTopCorner + new Vector2(0, 10 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Drag: " + dragEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 11 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Flick: " + flickEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 12 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "LongPress: " + longPressEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 13 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Composite: " + compositeEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 14 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Tap: " + tapEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 15 * (textHeight + TextSpaceY)), fontColor);

            spriteBatch.End();
        }

        private void DrawPointers(Tuple<Vector2, TimeSpan, int> tuple, float baseScale, Color baseColor)
        {
            var position = tuple.Item1;
            var duration = DrawTime.Total - tuple.Item2;

            var scale = (float)(0.2f * (1f - duration.TotalSeconds / displayPointerDuration.TotalSeconds));
            var pointerScreenPosition = new Vector2(position.X * screenSize.X, position.Y * screenSize.Y);

            spriteBatch.Draw(roundTexture, pointerScreenPosition, baseColor, 0, roundTextureSize / 2, scale * baseScale);
        }

        private async Task UpdateInputStates()
        {
            while (true)
            {
                await Script.NextFrame();

                var currentTime = DrawTime.Total;

                keyPressed = "";
                keyDown = "";
                keyReleased = "";
                mouseButtonPressed = "";
                mouseButtonDown = "";
                mouseButtonReleased = "";
                mouseWheelDelta = "";
                dragEvent = "";
                flickEvent = "";
                longPressEvent = "";
                compositeEvent = "";
                tapEvent = "";

                // Keyboard
                if (Input.HasKeyboard)
                {
                    foreach (var key in Input.KeyEvents.Where(keyEvent => keyEvent.Type == KeyEventType.Pressed))
                        keyPressed += key + ", ";

                    foreach (var key in Input.KeyDown)
                        keyDown += key + ", ";

                    foreach (var key in Input.KeyEvents.Where(keyEvent => keyEvent.Type == KeyEventType.Released))
                        keyReleased += key + ", ";
                }

                // Mouse
                if (Input.HasMouse)
                {
                    mousePosition = Input.MousePosition;
                    for (int i = 0; i <= (int)MouseButton.Extended2; i++)
                    {
                        var button = (MouseButton)i;
                        if (Input.IsMouseButtonPressed(button))
                            mouseButtonPressed += button + ", ";
                        if (Input.IsMouseButtonDown(button))
                            mouseButtonDown += button + ", ";
                        if (Input.IsMouseButtonReleased(button))
                            mouseButtonReleased += button + ", ";
                    }
                }
                mouseWheelDelta = Input.MouseWheelDelta.ToString();

                // Pointers
                if (Input.HasPointer)
                {
                    foreach (var pointerEvent in Input.PointerEvents)
                    {
                        switch (pointerEvent.State)
                        {
                            case PointerState.Down:
                                pointerPressed.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerState.Move:
                                pointerMoved.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerState.Up:
                                pointerReleased.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerState.Out:
                            case PointerState.Cancel:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    // remove too old pointer events
                    RemoveOldPointerEventInfo(pointerPressed);
                    RemoveOldPointerEventInfo(pointerMoved);
                    RemoveOldPointerEventInfo(pointerReleased);
                }

                // Gestures
                foreach (var gestureEvent in Input.GestureEvents)
                {
                    switch (gestureEvent.Type)
                    {
                        case GestureType.Drag:
                            var dragGestureEvent = (GestureEventDrag)gestureEvent;
                            dragEvent = "Position = " + dragGestureEvent.TotalTranslation;
                            break;
                        case GestureType.Flick:
                            lastFlickEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        case GestureType.LongPress:
                            lastLongPressEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        case GestureType.Composite:
                            var compositeGestureEvent = (GestureEventComposite)gestureEvent;
                            compositeEvent = "Rotation = " + compositeGestureEvent.TotalRotation + " - Scale = " + compositeGestureEvent.TotalScale + " - Position = " + compositeGestureEvent.TotalTranslation;
                            break;
                        case GestureType.Tap:
                            lastTapEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (currentTime - lastFlickEvent.Item2 < displayGestureDuration && lastFlickEvent.Item1 != null)
                {
                    var flickGestureEvent = (GestureEventFlick)lastFlickEvent.Item1;
                    flickEvent = " Start Position = " + flickGestureEvent.StartPosition + " - Speed = " + flickGestureEvent.AverageSpeed;
                }
                if (currentTime - lastLongPressEvent.Item2 < displayGestureDuration && lastLongPressEvent.Item1 != null)
                {
                    var longPressGestureEvent = (GestureEventLongPress)lastLongPressEvent.Item1;
                    longPressEvent = " Position = " + longPressGestureEvent.Position;
                }
                if (currentTime - lastTapEvent.Item2 < displayGestureDuration && lastTapEvent.Item1 != null)
                {
                    var tapGestureEvent = (GestureEventTap)lastTapEvent.Item1;
                    tapEvent = " Position = " + tapGestureEvent.TapPosition + " - number of taps = " + tapGestureEvent.NumberOfTaps;
                }
            }
        }

        // utility function to remove old pointer event from the queues
        private void RemoveOldPointerEventInfo(Queue<Tuple<Vector2, TimeSpan, int>> tuples)
        {
            while (tuples.Count > 0 && UpdateTime.Total - tuples.Peek().Item2 > displayPointerDuration)
                tuples.Dequeue();
        }

        // Override the Update function to quit the game when the user press escape
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyReleased(Keys.Escape))
                Exit();
        }

        [Test]
        public void RunSampleInputTest()
        {
            RunGameTest(new AdvancedInputTest());
        }

        public static void Main(string[] args)
        {
            using (var game = new AdvancedInputTest())
                game.Run();
        }
    }
}
