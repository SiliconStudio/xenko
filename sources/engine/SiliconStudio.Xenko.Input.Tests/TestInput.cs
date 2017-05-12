// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class TestInput : GameTestBase
    {
        public TestInput()
        {
            InputSourceSimulated.Enabled = true;
        }
        
        /// <summary>
        /// Checks keyboard press/release
        /// </summary>
        void TestPressRelease()
        {
            var events = Input.Events;
            var keyboard = InputSourceSimulated.Instance.Keyboard;

            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            
            // Test press
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.IsDown);
            Assert.IsTrue(keyEvent.RepeatCount == 0);
            Assert.IsTrue(keyEvent.Device == keyboard);

            // Check pressed/released states
            Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
            Assert.IsFalse(keyboard.IsKeyReleased(Keys.A));
            Assert.IsTrue(keyboard.IsKeyDown(Keys.A));

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(!keyEvent.IsDown);

            // Check pressed/released states
            Assert.IsFalse(keyboard.IsKeyPressed(Keys.A));
            Assert.IsTrue(keyboard.IsKeyReleased(Keys.A));
            Assert.IsFalse(keyboard.IsKeyDown(Keys.A));
        }

        /// <summary>
        /// Checks reported events and state when key repeats occur
        /// </summary>
        void TestRepeat()
        {
            var events = Input.Events;
            var keyboard = InputSourceSimulated.Instance.Keyboard;

            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            
            // Test press with release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.IsDown);
            Assert.IsTrue(keyEvent.RepeatCount == 3);
            Assert.IsTrue(keyEvent.Device == keyboard);

            // Check pressed/released states (Pressed events should still be sent when repeating)
            Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
            Assert.IsFalse(keyboard.IsKeyReleased(Keys.A));
            Assert.IsTrue(keyboard.IsKeyDown(Keys.A));

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(!keyEvent.IsDown);
        }

        /// <summary>
        /// Checks mouse and pointer events
        /// </summary>
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

            // Check pressed/released states
            Assert.IsTrue(mouse.IsButtonPressed(MouseButton.Left));
            Assert.IsFalse(mouse.IsButtonReleased(MouseButton.Left));
            Assert.IsTrue(mouse.IsButtonDown(MouseButton.Left));

            Assert.AreEqual(1, mouse.PressedPointers.Count);
            Assert.AreEqual(0, mouse.ReleasedPointers.Count);
            Assert.AreEqual(1, mouse.DownPointers.Count);
            
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

            // Check pressed/released states
            Assert.IsFalse(mouse.IsButtonPressed(MouseButton.Left));
            Assert.IsTrue(mouse.IsButtonReleased(MouseButton.Left));
            Assert.IsFalse(mouse.IsButtonDown(MouseButton.Left));

            Assert.AreEqual(0, mouse.PressedPointers.Count);
            Assert.AreEqual(1, mouse.ReleasedPointers.Count);
            Assert.AreEqual(0, mouse.DownPointers.Count);
        }

        /// <summary>
        /// Checks if the pressed/released states work correctly when the occur on the same frame
        /// </summary>
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
        
        /// <summary>
        /// Checks adding/removal of keyboard and mouse
        /// </summary>
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
                Assert.AreEqual(DeviceChangedEventType.Removed, args.Type);
                if (args.Device == InputSourceSimulated.Instance.Keyboard)
                    keyboardRemoved = true;
            };
            Input.DeviceAdded += (sender, args) =>
            {
                Assert.AreEqual(DeviceChangedEventType.Added, args.Type);
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

            // Test not crashing with no keyboard/mouse in a few update loops
            InputSourceSimulated.Instance.SetKeyboardConnected(false);
            InputSourceSimulated.Instance.SetMouseConnected(false);

            for(int i = 0; i < 3; i++)
                Input.Update(DrawTime);

            InputSourceSimulated.Instance.SetKeyboardConnected(true);
            InputSourceSimulated.Instance.SetMouseConnected(true);
        }

        /// <summary>
        /// Checks reported mouse delta and position with cursor position locked
        /// </summary>
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

        /// <summary>
        /// Checks adding/removing gamepads and index assignment
        /// </summary>
        void TestGamePad()
        {
            Assert.AreEqual(0, InputSourceSimulated.Instance.GamePads.Count);

            // Gamepad should only actually be added after update
            var gamePad0 = InputSourceSimulated.Instance.AddGamePad();
            Assert.AreEqual(0, Input.GamePadCount);

            Input.Update(DrawTime);
            
            Assert.AreEqual(1, Input.GamePadCount);

            // Add another gamepad
            var gamePad1 = InputSourceSimulated.Instance.AddGamePad();
            Input.Update(DrawTime);

            // Test automatic index assignment
            Assert.AreEqual(0, gamePad0.Index);
            Assert.AreEqual(1, gamePad1.Index);

            Assert.AreEqual(1, Input.GetGamePadsByIndex(0).Count());
            Assert.AreEqual(1, Input.GetGamePadsByIndex(1).Count());

            // Test putting both gamepads on the same index
            gamePad1.Index = 0;
            Assert.AreEqual(2, Input.GetGamePadsByIndex(0).Count());
            Assert.IsEmpty(Input.GetGamePadsByIndex(1));

            // Test reassign suggestions
            gamePad1.Index = Input.GetFreeGamePadIndex(gamePad1);
            gamePad0.Index = Input.GetFreeGamePadIndex(gamePad0);
            Assert.True(gamePad1.Index == 0 || gamePad0.Index == 0);
            Assert.True(gamePad0.Index != gamePad1.Index);

            // Gamepad should only actually be removed after update
            InputSourceSimulated.Instance.RemoveGamePad(gamePad0);
            InputSourceSimulated.Instance.RemoveGamePad(gamePad1);
            Assert.AreEqual(2, Input.GamePadCount);

            Input.Update(DrawTime);

            Assert.AreEqual(0, Input.GamePadCount);
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
            FrameGameSystem.Update(TestGamePad);
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