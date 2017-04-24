// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
                            log.Info($"Drag: [Params = {drag.State} {drag.NumberOfFinger} Time {drag.DeltaTime} {drag.TotalTime} Pos {drag.StartPosition} {drag.CurrentPosition} Transl {drag.DeltaTranslation} {drag.TotalTranslation} Speed {drag.AverageSpeed}");
                            break;
                        case GestureType.Flick:
                            var flick = (GestureEventFlick)gestureEvent;
                            log.Info($"Flick: [Params = {flick.State} {flick.NumberOfFinger} Time {flick.TotalTime} Pos {flick.StartPosition} {flick.CurrentPosition} Transl {flick.TotalTranslation} Speed {flick.AverageSpeed}");
                            break;
                        case GestureType.LongPress:
                            var longPressGesture = (GestureEventLongPress)gestureEvent;
                            log.Info($"A long press event has been detected. [Params = {longPressGesture.State} {longPressGesture.DeltaTime} {longPressGesture.NumberOfFinger} {longPressGesture.Position}");
                            break;
                        case GestureType.Composite:
                            var compGesture = (GestureEventComposite)gestureEvent;
                            log.Info($"Rotation: [Params = {compGesture.State} Time {compGesture.DeltaTime} {compGesture.TotalTime} angles {compGesture.DeltaRotation} {compGesture.TotalRotation} scale {compGesture.DeltaScale} {compGesture.TotalScale} Transl {compGesture.DeltaTranslation} {compGesture.TotalTranslation} Center {compGesture.CenterBeginningPosition} {compGesture.CenterCurrentPosition}");
                            break;
                        case GestureType.Tap:
                            var tapGesture = (GestureEventTap)gestureEvent;
                            log.Info($"Tap: [Params = Fingers {tapGesture.NumberOfFinger} Taps {tapGesture.NumberOfTaps} Time {tapGesture.DeltaTime} Position {tapGesture.TapPosition}");
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
