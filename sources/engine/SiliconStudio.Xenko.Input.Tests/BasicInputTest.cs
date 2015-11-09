// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Graphics.Regression;

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
            Input.ActivatedGestures.Add(new GestureConfigLongPress());
            Input.ActivatedGestures.Add(new GestureConfigLongPress { RequiredNumberOfFingers = 2 });
            Input.ActivatedGestures.Add(new GestureConfigLongPress { RequiredNumberOfFingers = 3 });

            // drag gestures
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Horizontal));
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Vertical));
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Free));
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Horizontal) { RequiredNumberOfFingers = 2 });
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Vertical) { RequiredNumberOfFingers = 2 });
            //Input.ActivatedGestures.Add(new GestureConfigDrag(GestureShape.Free) { RequiredNumberOfFingers = 2 });

            // flick gestures
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Horizontal));
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Vertical));
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Free));
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Horizontal) { RequiredNumberOfFingers = 2 });
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Vertical) { RequiredNumberOfFingers = 2 });
            //Input.ActivatedGestures.Add(new GestureConfigFlick(GestureShape.Free) { RequiredNumberOfFingers = 2 });

            // rotation gestures
            Input.ActivatedGestures.Add(new GestureConfigComposite());

            // taps gestures
            Input.ActivatedGestures.Add(new GestureConfigTap(1, 1));
            Input.ActivatedGestures.Add(new GestureConfigTap(2, 1));
            Input.ActivatedGestures.Add(new GestureConfigTap(1, 2));
            Input.ActivatedGestures.Add(new GestureConfigTap(2, 2));

            Script.AddTask(LogGestures);

            return Task.FromResult(0);
        }

        private async Task LogGestures()
        {
            while (true)
            {
                await Script.NextFrame();

                foreach (var gestureEvent in Input.GestureEvents)
                {
                    switch (gestureEvent.Type)
                    {
                        case GestureType.Drag:
                            var drag = (GestureEventDrag)gestureEvent;
                            log.Info("Drag: [Params = {0} {1} Time {2} {3} Pos {4} {5} Transl {6} {7} Speed {8}", drag.State, drag.NumberOfFinger, drag.DeltaTime, drag.TotalTime, 
                                drag.StartPosition, drag.CurrentPosition, drag.DeltaTranslation, drag.TotalTranslation, drag.AverageSpeed);
                            break;
                        case GestureType.Flick:
                            var flick = (GestureEventFlick)gestureEvent;
                            log.Info("Flick: [Params = {0} {1} Time {2} Pos {3} {4} Transl {5} Speed {6}", flick.State, flick.NumberOfFinger, flick.TotalTime,
                                flick.StartPosition, flick.CurrentPosition, flick.TotalTranslation, flick.AverageSpeed);
                            break;
                        case GestureType.LongPress:
                            var longPressGesture = (GestureEventLongPress)gestureEvent;
                            log.Info("A long press event has been detected. [Params = {0} {1} {2} {3}", longPressGesture.State, longPressGesture.DeltaTime, longPressGesture.NumberOfFinger, longPressGesture.Position);
                            break;
                        case GestureType.Composite:
                            var compGesture = (GestureEventComposite)gestureEvent;
                            log.Info("Rotation: [Params = {0} Time {1} {2} angles {3} {4} scale {5} {6} Transl {7} {8} Center {9} {10}", compGesture.State, compGesture.DeltaTime, compGesture.TotalTime,
                                            compGesture.DeltaRotation, compGesture.TotalRotation, compGesture.DeltaScale, compGesture.TotalScale, 
                                            compGesture.DeltaTranslation, compGesture.TotalTranslation, compGesture.CenterBeginningPosition, compGesture.CenterCurrentPosition);
                            break;
                        case GestureType.Tap:
                            var tapGesture = (GestureEventTap)gestureEvent;
                            log.Info("Tap: [Params = Fingers {0} Taps {1} Time {2} Position {3}", tapGesture.NumberOfFinger, tapGesture.NumberOfTaps, tapGesture.DeltaTime, tapGesture.TapPosition);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
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
