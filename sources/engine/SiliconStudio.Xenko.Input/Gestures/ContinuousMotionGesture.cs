// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public abstract class ContinuousMotionGesture : PointerGestureBase
    {
        protected bool GestureBeganEventSent { get; private set; }
        
        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            UpdateGestureStartEndStatus(true, id, pos);
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            if (!FingerIdsToLastPos.ContainsKey(id))
                return;

            UpdateGestureStartEndStatus(false, id, pos);
        }

        private void UpdateGestureStartEndStatus(bool isKeyDown, int id, Vector2 pos)
        {
            var gestureWasStarted = HasGestureStarted;
            HasGestureStarted = (CurrentFingerCount + (isKeyDown ? 1 : -1) == RequiredFingerCount);

            UpdateFingerDictionaries(isKeyDown, id, pos);

            if (HasGestureStarted) // beginning of a new gesture
            {
                InitializeGestureVariables();
            }
            else if (gestureWasStarted && GestureBeganEventSent) // end of the current gesture
            {
                AddGestureEventToCurrentList(PointerGestureEventType.Ended);
            }
        }

        protected abstract void InitializeGestureVariables();

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            // update current finger positions.
            foreach (var id in fingerIdsToMovePos.Keys)
                FingerIdsToLastPos[id] = fingerIdsToMovePos[id];

            if (!HasGestureStarted) // nothing more to do is the gesture has not started yet
                return;

            UpdateGestureVarsAndPerfomChecks();

            if (!GestureBeganEventSent && HasGestureStarted && GestureBeginningConditionFulfilled())
            {
                AddGestureEventToCurrentList(PointerGestureEventType.Began);
                GestureBeganEventSent = true;
            }

            if (GestureBeganEventSent)
                AddGestureEventToCurrentList(HasGestureStarted ? PointerGestureEventType.Changed : PointerGestureEventType.Ended);
        }

        protected abstract void UpdateGestureVarsAndPerfomChecks();

        protected abstract bool GestureBeginningConditionFulfilled();

        private void UpdateFingerDictionaries(bool isKeyDown, int id, Vector2 pos)
        {
            if (isKeyDown)
            {
                FingerIdToBeginPositions[id] = pos;
                FingerIdsToLastPos[id] = pos;
            }
            else
            {
                FingerIdToBeginPositions.Remove(id);
                FingerIdsToLastPos.Remove(id);
            }
        }

        protected virtual void AddGestureEventToCurrentList(PointerGestureEventType eventType)
        {
            ElapsedSinceLast = TimeSpan.Zero;

            if (eventType == PointerGestureEventType.Ended)
                GestureBeganEventSent = false;
        }
    }
}