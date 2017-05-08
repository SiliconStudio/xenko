// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal sealed class GestureRecognizerTap : GestureRecognizer
    {
        private GestureConfigTap ConfigTap { get { return (GestureConfigTap)Config; } }

        private int currentNumberOfTaps;

        private TimeSpan elapsedSinceTakeOff;

        private TimeSpan elapsedSinceDown;

        private bool isTapDown;

        private int maxNbOfFingerTouched;

        public GestureRecognizerTap(GestureConfigTap configuration, float screenRatio)
            :base(configuration, screenRatio)
        {
        }

        protected override void ProcessPointerEventsImpl(TimeSpan deltaTime, List<PointerEvent> events)
        {
            if (isTapDown)
                elapsedSinceDown += deltaTime;
            else
                elapsedSinceTakeOff += deltaTime;

            AnalysePointerEvents(events);

            // examine the tap gesture times and determine if the gesture has ended.
            if (HasGestureStarted && (elapsedSinceDown > ConfigTap.MaximumPressTime || elapsedSinceTakeOff > ConfigTap.MaximumTimeBetweenTaps))
            {
                // The Tap gesture has finished because of time restriction 
                EndCurrentTap();
            }
        }

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            if (!FingerIdToBeginPositions.ContainsKey(id))
                FingerIdToBeginPositions[id] = pos;

            FingerIdsToLastPos[id] = pos;

            maxNbOfFingerTouched = Math.Max(maxNbOfFingerTouched, NbOfFingerOnScreen);

            isTapDown = true;

            if (NbOfFingerOnScreen == 1)
            {
                elapsedSinceTakeOff = TimeSpan.Zero;
                elapsedSinceDown = TimeSpan.Zero;
                HasGestureStarted = true;
            }
            
            if(HasGestureStarted && maxNbOfFingerTouched > ConfigTap.RequiredNumberOfFingers)
                EndCurrentTap();

            if (HasGestureStarted && !HadFingerAtThatPosition(pos))
            {
                EndCurrentTap();

                maxNbOfFingerTouched = NbOfFingerOnScreen;
                elapsedSinceTakeOff = TimeSpan.Zero;
                elapsedSinceDown = TimeSpan.Zero;
                HasGestureStarted = true;
                foreach (var key in FingerIdsToLastPos.Keys)
                    FingerIdToBeginPositions[key] = FingerIdsToLastPos[key];
            }
        }

        private bool HadFingerAtThatPosition(Vector2 pos)
        {
            // finger ids can change during between two different taps so we have to check all the begin position
            foreach (var beginPos in FingerIdToBeginPositions.Values)
            {
                if ((pos - beginPos).Length() < ConfigTap.MaximumDistanceTaps)
                    return true;
            }

            return false;
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            if (!HasGestureStarted) // nothing to do is the gesture has not started yet
                return;

            foreach (var id in fingerIdsToMovePos.Keys)
            {
                if (!FingerIdsToLastPos.ContainsKey(id))
                    continue;

                if ((fingerIdsToMovePos[id] - FingerIdsToLastPos[id]).Length() > ConfigTap.MaximumDistanceTaps)
                {
                    EndCurrentTap();
                    return;
                }
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            if (!FingerIdsToLastPos.ContainsKey(id))
                return;

            FingerIdsToLastPos.Remove(id);

            if (NbOfFingerOnScreen == 0)
            {
                elapsedSinceTakeOff = TimeSpan.Zero;
                isTapDown = false;

                if (HasGestureStarted && maxNbOfFingerTouched == ConfigTap.RequiredNumberOfFingers)
                    ++currentNumberOfTaps;

                maxNbOfFingerTouched = 0;
            }
        }
        
        private void EndCurrentTap()
        {
            // add the gesture to the tap event list if the number of tap requirement is fulfilled
            if (currentNumberOfTaps == ConfigTap.RequiredNumberOfTaps)
            {
                var tapMeanPosition = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                CurrentGestureEvents.Add(new GestureEventTap(ElapsedSinceBeginning, ConfigTap.RequiredNumberOfFingers, currentNumberOfTaps, NormalizeVector(tapMeanPosition)));
            }

            currentNumberOfTaps = 0;

            HasGestureStarted = false;
            FingerIdToBeginPositions.Clear();
        }
    }
}
