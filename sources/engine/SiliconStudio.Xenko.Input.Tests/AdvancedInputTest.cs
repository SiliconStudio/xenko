// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Tests
{
    class AdvancedInputTest : InputTestBase
    {
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
        
        private Tuple<FlickEventArgs, TimeSpan> lastFlickEvent = new Tuple<FlickEventArgs, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<LongPressEventArgs, TimeSpan> lastLongPressEvent = new Tuple<LongPressEventArgs, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<TapEventArgs, TimeSpan> lastTapEvent = new Tuple<TapEventArgs, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<DragEventArgs, TimeSpan> lastDragEvent = new Tuple<DragEventArgs, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<CompositeEventArgs, TimeSpan> lastCompositeEvent = new Tuple<CompositeEventArgs, TimeSpan>(null, TimeSpan.Zero);

        private DragGesture dragGesture = new DragGesture();
        private FlickGesture flickGesture = new FlickGesture();
        private LongPressGesture longPressGesture = new LongPressGesture();
        private CompositeGesture compositeGesture = new CompositeGesture();
        private TapGesture tapGesture = new TapGesture();

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


            displayPointerDuration = TimeSpan.FromSeconds(1.5f);
            displayGestureDuration = TimeSpan.FromSeconds(1f);
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Add the gestures to the input manager
            Input.Gestures.Add(dragGesture);
            Input.Gestures.Add(flickGesture);
            Input.Gestures.Add(longPressGesture);
            Input.Gestures.Add(compositeGesture);
            Input.Gestures.Add(tapGesture);

            compositeGesture.Changed += (sender, args) =>
            {
                lastCompositeEvent = Tuple.Create(args, DrawTime.Total);
            };

            dragGesture.Drag += (sender, args) =>
            {
                lastDragEvent = Tuple.Create(args, DrawTime.Total);
            };

            flickGesture.Flick += (sender, args) =>
            {
                lastFlickEvent = Tuple.Create(args, DrawTime.Total);
            };

            longPressGesture.LongPress += (sender, args) =>
            {
                lastLongPressEvent = Tuple.Create(args, DrawTime.Total);
            };

            tapGesture.Tap += (sender, args) =>
            {
                lastTapEvent = Tuple.Create(args, DrawTime.Total);
            };

            // add a task to the task scheduler that will be executed asynchronously 
            Script.AddTask(UpdateInputStates);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // clear the screen
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.White);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);

            BeginSpriteBatch();

            // render the keyboard key states
            WriteLine("Keyboard:");
            WriteLine("Key pressed: " + keyPressed, 1);
            WriteLine("Key down: " + keyDown, 1);
            WriteLine("Key released: " + keyReleased, 1);

            // render the mouse key states
            WriteLine("Mouse :");
            WriteLine("Mouse position: " + mousePosition, 1);
            WriteLine("Mouse button pressed: " + mouseButtonPressed, 1);
            WriteLine("Mouse button down: " + mouseButtonDown, 1);
            WriteLine("Mouse button released: " + mouseButtonReleased, 1);
            WriteLine("Mouse wheel delta: " + mouseWheelDelta, 1);
                
            // render the pointer states
            foreach (var tuple in pointerPressed)
                DrawPointers(tuple, 1.5f, Color.Blue);
            foreach (var tuple in pointerMoved)
                DrawPointers(tuple, 1f, Color.Green);
            foreach (var tuple in pointerReleased)
                DrawPointers(tuple, 2f, Color.Red);

            // render the gesture states
            WriteLine("Gestures :");
            WriteLine("Drag: " + dragEvent, 1);
            WriteLine("Flick: " + flickEvent, 1);
            WriteLine("LongPress: " + longPressEvent, 1);
            WriteLine("Composite: " + compositeEvent, 1);
            WriteLine("Tap: " + tapEvent, 1);

            DrawCursor();

            EndSpriteBatch();
        }

        private void DrawPointers(Tuple<Vector2, TimeSpan, int> tuple, float baseScale, Color baseColor)
        {
            var position = tuple.Item1;
            var duration = DrawTime.Total - tuple.Item2;

            var scale = (float)(0.2f * (1f - duration.TotalSeconds / displayPointerDuration.TotalSeconds));
            var pointerScreenPosition = new Vector2(position.X * ScreenSize.X, position.Y * ScreenSize.Y);

            SpriteBatch.Draw(RoundTexture, pointerScreenPosition, baseColor, 0, RoundTextureSize / 2, scale * baseScale);
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
                    keyPressed = string.Join(", ", Input.KeyEvents.Where(keyEvent => keyEvent.IsDown));
                    keyDown = string.Join(", ", Input.DownKeys);
                    keyReleased = string.Join(", ", Input.KeyEvents.Where(keyEvent => !keyEvent.IsDown));
                }

                // Mouse
                if (Input.HasMouse)
                {
                    mousePosition = Input.MousePosition;
                    for (int i = 0; i <= (int)MouseButton.Extended2; i++)
                    {
                        var button = (MouseButton)i;
                        if (Input.IsMouseButtonPressed(button))
                        {
                            if (mouseButtonPressed.Length > 0)
                                mouseButtonPressed += ", ";
                            mouseButtonPressed += button;
                        }
                        if (Input.IsMouseButtonDown(button))
                        {
                            if (mouseButtonDown.Length > 0)
                                mouseButtonDown += ", ";
                            mouseButtonDown += button;
                        }
                        if (Input.IsMouseButtonReleased(button))
                        {
                            if (mouseButtonReleased.Length > 0)
                                mouseButtonReleased += ", ";
                            mouseButtonReleased += button;
                        }
                    }
                }
                mouseWheelDelta = Input.MouseWheelDelta.ToString();

                // Pointers
                if (Input.HasPointer)
                {
                    foreach (var pointerEvent in Input.PointerEvents)
                    {
                        switch (pointerEvent.EventType)
                        {
                            case PointerEventType.Pressed:
                                pointerPressed.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerEventType.Moved:
                                pointerMoved.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerEventType.Released:
                                pointerReleased.Enqueue(Tuple.Create(pointerEvent.Position, currentTime, pointerEvent.PointerId));
                                break;
                            case PointerEventType.Canceled:
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
                if (currentTime - lastFlickEvent.Item2 < displayGestureDuration && lastFlickEvent.Item1 != null)
                {
                    var args = lastFlickEvent.Item1;
                    flickEvent = $"Start Position = {args.StartPosition} - Speed = {args.AverageSpeed} - EventType = {args.EventType}";
                }
                if (currentTime - lastLongPressEvent.Item2 < displayGestureDuration && lastLongPressEvent.Item1 != null)
                {
                    var args = lastLongPressEvent.Item1;
                    longPressEvent = $"Position = {args.Position} - EventType = {args.EventType}";
                }
                if (currentTime - lastTapEvent.Item2 < displayGestureDuration && lastTapEvent.Item1 != null)
                {
                    var args = lastTapEvent.Item1;
                    tapEvent = $"Position = {args.TapPosition} - number of taps = {args.TapCount} - EventType = {args.EventType}";
                }
                if (currentTime - lastDragEvent.Item2 < displayGestureDuration && lastDragEvent.Item1 != null)
                {
                    var args = lastDragEvent.Item1;
                    dragEvent = $"Position = {args.TotalTranslation} - EventType = {args.EventType}";
                }
                if (currentTime - lastCompositeEvent.Item2 < displayGestureDuration && lastCompositeEvent.Item1 != null)
                {
                    var args = lastCompositeEvent.Item1;
                    compositeEvent = $"Rotation = {args.TotalRotation} - Scale = {args.TotalScale} - Position = {args.TotalTranslation} - EventType = {args.EventType}";
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
