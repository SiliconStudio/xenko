// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal sealed class GestureRecognizerFlick : GestureRecognizer
    {
        public GestureRecognizerFlick(GestureConfigFlick config, float screenRatio)
            : base(config, screenRatio)
        {
        }

        private GestureConfigFlick ConfigFlick { get { return (GestureConfigFlick)Config; } }

        protected override GestureEvent NewEventFactory()
        {
            return new GestureEventFlick();
        }

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions[id] = pos;
            FingerIdsToLastPos[id] = pos;
            HasGestureStarted = (NbOfFingerOnScreen == ConfigFlick.RequiredNumberOfFingers);
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            if (!HasGestureStarted)
                return;

            foreach (var id in fingerIdsToMovePos.Keys)
            {
                var newPos = fingerIdsToMovePos[id];
                
                // check that the shape of the flick is respected, stop the gesture if it is not the case
                if (ConfigFlick.FlickShape != GestureShape.Free)
                {
                    var compIndex = ConfigFlick.FlickShape == GestureShape.Horizontal? 1:0;
                    if (Math.Abs(newPos[compIndex] - FingerIdToBeginPositions[id][compIndex]) > ConfigFlick.AllowedErrorMargins[compIndex])
                        HasGestureStarted = false;
                }

                // Update the last position of the finger
                FingerIdsToLastPos[id] = newPos;
            }

            if (HasGestureStarted)
            {
                // trigger the event if the conditions are fulfilled
                var startPos = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                var currPos = ComputeMeanPosition(FingerIdsToLastPos.Values);
                var translDist = (currPos - startPos).Length();
                if (translDist > ConfigFlick.MinimumFlickLength && translDist / ElapsedSinceBeginning.TotalSeconds > ConfigFlick.MinimumAverageSpeed)
                {
                    var evt = CurrentGestureEvents.Add() as GestureEventFlick;
                    evt.Set(ConfigFlick.RequiredNumberOfFingers, ElapsedSinceBeginning, ConfigFlick.FlickShape, NormalizeVector(startPos), NormalizeVector(currPos));
                    HasGestureStarted = false;
                }
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions.Remove(id);
            FingerIdsToLastPos.Remove(id);
            HasGestureStarted = (NbOfFingerOnScreen == ConfigFlick.RequiredNumberOfFingers);
        }
    }
}
