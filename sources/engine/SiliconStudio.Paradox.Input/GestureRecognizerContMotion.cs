// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// A gesture recognizer for continuous motions.
    /// </summary>
    internal abstract class GestureRecognizerContMotion : GestureRecognizer
    {
        protected bool GestureBeganEventSent { get; private set; }

        protected GestureRecognizerContMotion(GestureConfig config, float screenRatio)
            : base(config, screenRatio)
        {
        }

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
            HasGestureStarted = (NbOfFingerOnScreen + (isKeyDown?1:-1)  == Config.RequiredNumberOfFingers);

            if (HasGestureStarted) // beginning of a new drag gesture
            {
                UpdateFingerDictionaties(isKeyDown, id, pos);

                InitializeGestureVariables();
            }
            else // end of the current drag gesture
            {
                if (gestureWasStarted && GestureBeganEventSent)
                    AddGestureEventToCurrentList(GestureState.Ended);

                UpdateFingerDictionaties(isKeyDown, id, pos);
            }
        }

        protected abstract void InitializeGestureVariables();


        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            if (!HasGestureStarted) // nothing to do is the gesture has not started yet
                return;

            foreach (var id in fingerIdsToMovePos.Keys)
                FingerIdsToLastPos[id] = fingerIdsToMovePos[id];

            UpdateGestureVarsAndPerfomChecks();

            if (!GestureBeganEventSent && HasGestureStarted && GestureBeginningConditionFulfilled())
            {
                AddGestureEventToCurrentList(GestureState.Began);
                GestureBeganEventSent = true;
            }

            if (GestureBeganEventSent)
                AddGestureEventToCurrentList(HasGestureStarted ? GestureState.Changed : GestureState.Ended);
        }

        protected abstract void UpdateGestureVarsAndPerfomChecks();

        protected abstract bool GestureBeginningConditionFulfilled();

        private void UpdateFingerDictionaties(bool isKeyDown, int id, Vector2 pos)
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

        protected virtual void AddGestureEventToCurrentList(GestureState state)
        {
            ElapsedSinceLast = TimeSpan.Zero;

            if (state == GestureState.Ended)
                GestureBeganEventSent = false;
        }
    }
}