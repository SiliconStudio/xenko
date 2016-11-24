// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input.Gestures;

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

            Input.ActivatedGestures.Add(tap);

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

            Input.ActivatedGestures.Remove(tap);
        }

        void TestCompositeGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            CompositeGesture composite = new CompositeGesture();

            Input.ActivatedGestures.Add(composite);

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

            Input.ActivatedGestures.Remove(composite);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            
            FrameGameSystem.Update(TestTapGesture);
            FrameGameSystem.Update(TestCompositeGesture);
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