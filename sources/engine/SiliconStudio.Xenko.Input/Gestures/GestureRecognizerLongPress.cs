// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal sealed class GestureRecognizerLongPress : GestureRecognizer
    {
        public GestureRecognizerLongPress(GestureConfigLongPress config, float screenRatio)
            :base(config, screenRatio)
        {
        }

        protected override int NbOfFingerOnScreen
        {
            get { return FingerIdToBeginPositions.Count; }
        }

        private GestureConfigLongPress ConfigLongPress { get { return (GestureConfigLongPress)Config; } }
        
        protected override GestureEvent NewEventFactory()
        {
            return new GestureEventLongPress();
        }

        protected override void ProcessPointerEventsImpl(TimeSpan deltaTime, List<PointerEvent> events)
        {
            AnalysePointerEvents(events);

            if (HasGestureStarted && ElapsedSinceBeginning >= ConfigLongPress.RequiredPressTime)
            {
                var avgPosition = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                var evt = CurrentGestureEvents.Add() as GestureEventLongPress;
                evt.Set(ConfigLongPress.RequiredNumberOfFingers, ElapsedSinceBeginning, NormalizeVector(avgPosition));
                HasGestureStarted = false;
            }
        }

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions[id] = pos;
            HasGestureStarted = (NbOfFingerOnScreen == ConfigLongPress.RequiredNumberOfFingers);
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            foreach (var id in fingerIdsToMovePos.Keys)
            {
                var dist = (fingerIdsToMovePos[id] - FingerIdToBeginPositions[id]).Length();
                if (dist > ConfigLongPress.MaximumTranslationDistance)
                    HasGestureStarted = false;
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions.Remove(id);
            HasGestureStarted = (NbOfFingerOnScreen == ConfigLongPress.RequiredNumberOfFingers);
        }
    }
}
