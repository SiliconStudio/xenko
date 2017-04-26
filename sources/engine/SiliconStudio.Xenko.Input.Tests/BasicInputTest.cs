// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Tests
{
    class BasicInputTest : GameTestBase
    {
        private readonly Logger log = GlobalLogger.GetLogger("InputTests");

        public BasicInputTest()
        {
            CurrentVersion = 1;
        }

        protected override Task LoadContent()
        {
            // long press gestures
            AddGesture(new LongPressGesture());
            AddGesture(new LongPressGesture { RequiredFingerCount = 2 });
            AddGesture(new LongPressGesture { RequiredFingerCount = 3 });

            // drag gestures
            AddGesture(new DragGesture(GestureShape.Horizontal));
            AddGesture(new DragGesture(GestureShape.Vertical));
            AddGesture(new DragGesture(GestureShape.Free));
            AddGesture(new DragGesture(GestureShape.Horizontal) { RequiredFingerCount = 2 });
            AddGesture(new DragGesture(GestureShape.Vertical) { RequiredFingerCount = 2 });
            AddGesture(new DragGesture(GestureShape.Free) { RequiredFingerCount = 2 });

            // flick gestures
            AddGesture(new FlickGesture(GestureShape.Horizontal));
            AddGesture(new FlickGesture(GestureShape.Vertical));
            AddGesture(new FlickGesture(GestureShape.Free));
            AddGesture(new FlickGesture(GestureShape.Horizontal) { RequiredFingerCount = 2 });
            AddGesture(new FlickGesture(GestureShape.Vertical) { RequiredFingerCount = 2 });
            AddGesture(new FlickGesture(GestureShape.Free) { RequiredFingerCount = 2 });

            // rotation gestures
            AddGesture(new CompositeGesture());

            // taps gestures
            AddGesture(new TapGesture(1, 1));
            AddGesture(new TapGesture(2, 1));
            AddGesture(new TapGesture(1, 2));
            AddGesture(new TapGesture(2, 2));

            Script.AddTask(LogGestures);

            return Task.FromResult(0);
        }

        void AddGesture(DragGesture gesture)
        {
            gesture.Drag += (sender, args) =>
            {
                log.Info("Drag: [Params = {0} {1} Time {2} {3} Pos {4} {5} Transl {6} {7} Speed {8}", args.EventType, args.FingerCount, args.DeltaTime, args.TotalTime,
                                    args.StartPosition, args.CurrentPosition, args.DeltaTranslation, args.TotalTranslation, args.AverageSpeed);
            };
            Input.Gestures.Add(gesture);
        }
        void AddGesture(FlickGesture gesture)
        {
            gesture.Flick += (sender, args) =>
            {
                log.Info("Flick: [Params = {0} {1} Time {2} Pos {3} {4} Transl {5} Speed {6}", args.EventType, args.FingerCount, args.TotalTime,
                    args.StartPosition, args.CurrentPosition, args.TotalTranslation, args.AverageSpeed);
            };
            Input.Gestures.Add(gesture);
        }
        void AddGesture(LongPressGesture gesture)
        {
            gesture.LongPress += (sender, args) =>
            {
                log.Info("A long press event has been detected. [Params = {0} {1} {2} {3}", args.EventType, args.DeltaTime, args.FingerCount, args.Position);
            };
            Input.Gestures.Add(gesture);
        }
        void AddGesture(CompositeGesture gesture)
        {
            gesture.Changed += (sender, args) =>
            {
                log.Info("Rotation: [Params = {0} Time {1} {2} angles {3} {4} scale {5} {6} Transl {7} {8} Center {9} {10}", args.EventType, args.DeltaTime, args.TotalTime,
                                                args.DeltaRotation, args.TotalRotation, args.DeltaScale, args.TotalScale,
                                                args.DeltaTranslation, args.TotalTranslation, args.CenterBeginningPosition, args.CenterCurrentPosition);
            };
            Input.Gestures.Add(gesture);
        }
        void AddGesture(TapGesture gesture)
        {
            gesture.Tap += (sender, args) =>
            {
                log.Info("Tap: [Params = Fingers {0} Taps {1} Time {2} Position {3}", args.FingerCount, args.TapCount, args.DeltaTime, args.TapPosition);
            };
            Input.Gestures.Add(gesture);
        }

        private async Task LogGestures()
        {
            while (true)
            {
                await Script.NextFrame();
            }
        }
        
        [Test]
        public void RunBasicInputTest()
        {
            RunGameTest(new BasicInputTest());
        }

        public static void Main(string[] args)
        {
             using(var game = new BasicInputTest())
                game.Run();
        }
    }
}
