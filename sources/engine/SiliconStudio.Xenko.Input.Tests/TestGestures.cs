// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input.Gestures;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class TestGestures : GameTestBase
    {
        public TestGestures()
        {
            InputSourceSimulated.Enabled = true;
        }

        void TestTapGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            TapGesture tap = new TapGesture(2, 1);

            Input.Gestures.Add(tap);

            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2)));

            // Test tap occured
            Assert.AreEqual(1, tap.Events.Count);
            var tapEvent = (TapEventArgs)tap.Events[0];
            Assert.AreEqual(mouse.Position, tapEvent.TapPosition);
            Assert.AreEqual(2, tapEvent.DeltaTime.Seconds);
            Assert.AreEqual(PointerGestureEventType.Occurred, tapEvent.EventType);

            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2))); // Wait too long
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2)));

            // Test no tap occured
            Assert.AreEqual(0, tap.Events.Count);

            Input.Gestures.Remove(tap);
        }

        void TestCompositeGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            CompositeGesture composite = new CompositeGesture();

            Input.Gestures.Add(composite);

            // Starting position
            //
            //    (1)     (2)
            //
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.6f, 0.5f), 1);
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.4f, 0.5f), 2);
            Input.Update(DrawTime);
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.6f, 0.5f), 1);
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.4f, 0.5f), 2);
            Input.Update(DrawTime);

            // Check pointer state
            Assert.AreEqual(3, mouse.PointerPoints.Count);
            Assert.AreEqual(new Vector2(0.6f, 0.5f), mouse.PointerPoints[1].Position);
            Assert.IsTrue(mouse.PointerPoints[1].IsDown);

            // Ending position
            //        (1)
            //       /   /
            //        (2)
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 0.4f), 1);
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 0.6f), 2);
            Input.Update(DrawTime);

            Assert.IsNotEmpty(composite.Events);
            var evt = (CompositeEventArgs)composite.Events.Last();

            // Check reported rotation
            Assert.AreEqual(PointerGestureEventType.Changed, evt.EventType);
            Assert.AreEqual(-Math.PI * 0.5f, evt.TotalRotation, 0.01f); // 1/4 clockwise rotation

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.4f), 1);
            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.6f), 2);
            Input.Update(DrawTime);

            // Check ended
            Assert.IsNotEmpty(composite.Events);
            evt = (CompositeEventArgs)composite.Events.Last();
            Assert.AreEqual(PointerGestureEventType.Ended, evt.EventType);

            Input.Update(DrawTime);

            // Check empty event list
            Assert.IsEmpty(composite.Events);

            Input.Gestures.Remove(composite);
        }

        void TestDragGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            DragGesture hdrag = new DragGesture(GestureShape.Horizontal);
            Input.Gestures.Add(hdrag);

            // Should only trigger when horizontal movement is detected, try a vertical drag
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(hdrag.Events);

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 1.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(hdrag.Events);
            
            // Try a horizontal drag
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.0f, 0.0f));
            Input.Update(DrawTime);
            
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            {
                Assert.IsNotEmpty(hdrag.Events);
                var dragEvent = (DragEventArgs)hdrag.Events[0];
                Assert.AreEqual(PointerGestureEventType.Began, dragEvent.EventType);
                Assert.AreEqual(new Vector2(0.5f, 0.0f), dragEvent.DeltaTranslation);
                Assert.AreEqual(new Vector2(0.5f, 0.0f), dragEvent.TotalTranslation);
            }

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(1.0f, 0.0f));
            Input.Update(DrawTime);
            {
                Assert.IsNotEmpty(hdrag.Events);
                var dragEvent = (DragEventArgs)hdrag.Events[0];
                Assert.AreEqual(PointerGestureEventType.Ended, dragEvent.EventType);
                Assert.AreEqual(new Vector2(0.5f, 0.0f), dragEvent.TotalTranslation);
                Assert.AreEqual(new Vector2(0.5f, 0.0f), dragEvent.CurrentPosition);
            }
        }

        void TestFlickGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            FlickGesture flick = new FlickGesture(GestureShape.Free) { MinimumFlickLength = 0.2f, RequiredFingerCount = 1 };
            Input.Gestures.Add(flick);
            
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(flick.Events);

            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 1.0f));
            Input.Update(DrawTime);
            {
                Assert.IsNotEmpty(flick.Events);
                var flickEvent = (FlickEventArgs)flick.Events[0];
                Assert.AreEqual(new Vector2(0.0f, 1.0f), flickEvent.DeltaTranslation);
            }

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 1.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(flick.Events);
        }

        void TestLongPressGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            LongPressGesture longPress = new LongPressGesture() { RequiredPressTime = TimeSpan.FromMilliseconds(100) };
            Input.Gestures.Add(longPress);

            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(longPress.Events);

            {
                // Too short hold time
                var ts = TimeSpan.FromMilliseconds(90);
                Input.Update(new GameTime(ts, ts));
                Assert.IsEmpty(longPress.Events);
            }

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(longPress.Events);

            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(longPress.Events);

            {
                // Long enough hold time
                var ts = TimeSpan.FromMilliseconds(200);
                Input.Update(new GameTime(ts, ts));
                Assert.IsNotEmpty(longPress.Events);
                var longPressEvent = (LongPressEventArgs)longPress.Events[0];
                Assert.AreEqual(new Vector2(0.5f, 0.0f), longPressEvent.Position);
            }

            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.0f));
            Input.Update(DrawTime);
            Assert.IsEmpty(longPress.Events);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            
            FrameGameSystem.Update(TestTapGesture);
            FrameGameSystem.Update(TestCompositeGesture);
            FrameGameSystem.Update(TestDragGesture);
            FrameGameSystem.Update(TestFlickGesture);
            FrameGameSystem.Update(TestLongPressGesture);
        }

        [Test]
        public static void RunGesturesTest()
        {
            RunGameTest(new TestGestures());
        }

        public static void Main()
        {
            RunGesturesTest();
        }
    }
}