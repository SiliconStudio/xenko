// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class TestInput : GameTestBase
    {
        public TestInput()
        {
            InputSourceSimulated.Enabled = true;
        }
        
        void TestPressRelease()
        {
            var events = Input.Events;
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);

            var keyboard = InputSourceSimulated.Instance.Keyboard;

            // Test press
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.IsDown);
            Assert.IsTrue(keyEvent.RepeatCount == 0);
            Assert.IsTrue(keyEvent.Device == keyboard);

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(!keyEvent.IsDown);
        }

        void TestRepeat()
        {
            var events = Input.Events;
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);

            var keyboard = InputSourceSimulated.Instance.Keyboard;

            // Test press with release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.IsDown);
            Assert.IsTrue(keyEvent.RepeatCount == 3);
            Assert.IsTrue(keyEvent.Device == keyboard);

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(!keyEvent.IsDown);
        }

        void TestMouse()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            Vector2 targetPosition;
            mouse.SetPosition(targetPosition = new Vector2(0.5f, 0.5f));
            Input.Update(DrawTime);

            Assert.AreEqual(targetPosition, Input.MousePosition);

            mouse.SetPosition(targetPosition = new Vector2(0.6f, 0.5f));
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);

            // Check for pointer events (2, 1 move, 1 down)
            Assert.AreEqual(2, Input.PointerEvents.Count);
            Assert.AreEqual(PointerEventType.Moved, Input.PointerEvents[0].EventType);
            Assert.IsFalse(Input.PointerEvents[0].IsDown);

            // Check down
            Assert.AreEqual(PointerEventType.Pressed, Input.PointerEvents[1].EventType);
            Assert.IsTrue(Input.PointerEvents[1].IsDown);

            // Check delta
            Assert.AreEqual(new Vector2(0.1f, 0.0f), Input.PointerEvents[0].DeltaPosition);
            Assert.AreEqual(new Vector2(0.0f, 0.0f), Input.PointerEvents[1].DeltaPosition);

            // And the position after that, when the pointer goes down
            Assert.AreEqual(targetPosition, Input.PointerEvents[1].Position);

            // Check if new absolute delta matches the one reported in the input manager
            Assert.AreEqual(Input.PointerEvents[0].AbsoluteDeltaPosition, Input.AbsoluteMouseDelta);

            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(DrawTime);

            // Check up
            Assert.AreEqual(1, Input.PointerEvents.Count);
            Assert.AreEqual(PointerEventType.Released, Input.PointerEvents[0].EventType);
            Assert.IsFalse(Input.PointerEvents[0].IsDown);
        }

        void TestSingleFrameStates()
        {
            var keyboard = InputSourceSimulated.Instance.Keyboard;

            keyboard.SimulateDown(Keys.Space);
            keyboard.SimulateUp(Keys.Space);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsKeyPressed(Keys.Space));
            Assert.IsTrue(Input.IsKeyReleased(Keys.Space));
            Assert.IsFalse(Input.IsKeyDown(Keys.Space));


            var mouse = InputSourceSimulated.Instance.Mouse;

            mouse.SimulateMouseDown(MouseButton.Extended2);
            mouse.SimulateMouseUp(MouseButton.Extended2);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsMouseButtonPressed(MouseButton.Extended2));
            Assert.IsTrue(Input.IsMouseButtonReleased(MouseButton.Extended2));
            Assert.IsFalse(Input.IsMouseButtonDown(MouseButton.Extended2));
            
            mouse.SimulateMouseDown(MouseButton.Left);
            mouse.SimulateMouseUp(MouseButton.Left);
            mouse.SimulateMouseDown(MouseButton.Left);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsMouseButtonPressed(MouseButton.Left));
            Assert.IsTrue(Input.IsMouseButtonReleased(MouseButton.Left));
            Assert.IsTrue(Input.IsMouseButtonDown(MouseButton.Left));

            mouse.SimulateMouseUp(MouseButton.Left);
            Input.Update(DrawTime);
        }
        
        void TestConnectedDevices()
        {
            Assert.IsTrue(Input.HasMouse);
            Assert.NotNull(Input.Mouse);
            Assert.IsTrue(Input.HasPointer);
            Assert.NotNull(Input.Pointer);
            Assert.IsTrue(Input.HasKeyboard);
            Assert.NotNull(Input.Keyboard);
            Assert.IsFalse(Input.HasGamePad);
            Assert.IsFalse(Input.HasGameController);

            bool keyboardAdded = false;
            bool keyboardRemoved = false;

            Input.DeviceRemoved += (sender, args) =>
            {
                if (args.Device == InputSourceSimulated.Instance.Keyboard)
                    keyboardRemoved = true;
            };
            Input.DeviceAdded += (sender, args) =>
            {
                if (args.Device == InputSourceSimulated.Instance.Keyboard)
                    keyboardAdded = true;
            };

            // Check keyboard removal
            InputSourceSimulated.Instance.SetKeyboardConnected(false);
            Assert.IsTrue(keyboardRemoved);
            Assert.IsFalse(keyboardAdded);
            Assert.IsNull(Input.Keyboard);
            Assert.IsFalse(Input.HasKeyboard);

            // Check keyboard addition
            InputSourceSimulated.Instance.SetKeyboardConnected(true);
            Assert.IsTrue(keyboardAdded);
            Assert.IsNotNull(Input.Keyboard);
            Assert.IsTrue(Input.HasKeyboard);

            // Test not crashing with no keyboard/mouse
            InputSourceSimulated.Instance.SetKeyboardConnected(false);
            InputSourceSimulated.Instance.SetMouseConnected(false);

            Input.Update(DrawTime);
            Input.Update(DrawTime);
            Input.Update(DrawTime);

            InputSourceSimulated.Instance.SetKeyboardConnected(true);
            InputSourceSimulated.Instance.SetMouseConnected(true);
        }

        void TestLockedMousePosition()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;
            mouse.LockPosition(true);
            Input.Update(DrawTime);
            
            Assert.AreEqual(new Vector2(0.5f), mouse.Position);
            Input.Update(DrawTime);

            Assert.AreEqual(new Vector2(0.0f), mouse.Delta);

            // Validate mouse delta with locked position
            mouse.SetPosition(new Vector2(0.6f, 0.5f));
            Input.Update(DrawTime);
            Assert.AreEqual(new Vector2(0.1f, 0.0f), mouse.Delta);
            Input.Update(DrawTime);
            Assert.AreEqual(new Vector2(0.0f, 0.0f), mouse.Delta);
            Assert.AreEqual(new Vector2(0.5f, 0.5f), mouse.Position);

            mouse.UnlockPosition();
            
            // Validate mouse delta with unlocked position
            mouse.SetPosition(new Vector2(0.6f, 0.5f));
            Input.Update(DrawTime);
            Assert.AreEqual(new Vector2(0.1f, 0.0f), mouse.Delta);
            Assert.AreEqual(new Vector2(0.6f, 0.5f), mouse.Position);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Update(TestConnectedDevices);
            FrameGameSystem.Update(TestPressRelease);
            FrameGameSystem.Update(TestRepeat);
            FrameGameSystem.Update(TestMouse);
            FrameGameSystem.Update(TestLockedMousePosition);
            FrameGameSystem.Update(TestSingleFrameStates);
        }

        [Test]
        public static void RunInputTest()
        {
            RunGameTest(new TestInput());
        }

        public static void Main()
        {
            RunInputTest();
        }
    }
}