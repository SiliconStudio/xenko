// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A variable-count tap gesture recognizer
    /// </summary>
    public class TapGesture : PointerGestureBase
    {
        /// <summary>
        /// This value represents the maximum amount of time that the user can stay touching the screen before taking off its finger. 
        /// </summary>
        public TimeSpan MaximumPressTime;

        /// <summary>
        /// This value represents the maximum interval of time that can separate two touches of a same gesture. 
        /// By reducing this value, the system will tend to detect multi-touch gesture has several single touch gesture.
        /// By increasing this value, the system will tend to regroup distant (in time) single touch gestures into a multi-touch gesture.
        /// </summary>
        public TimeSpan MaximumTimeBetweenTaps;

        /// <summary>
        /// The value represents the maximum distance that can separate two touches of the same finger during the gesture.
        /// By reducing this value, the system will tend to detect multi-touch gesture has several single touch gesture.
        /// By increasing this value, the system will tend to regroup distant single touch gestures into a multi-touch gesture.
        /// </summary>
        public float MaximumDistanceTaps;

        /// <summary>
        /// This value represents the required number of successive user touches to trigger the gesture. For example: 1 for single touch, 2 for double touch, and so on...
        /// </summary>
        /// <remarks>This value is strictly positive.</remarks>
        public int RequiredTapCount;

        private int currentTapCount;
        private TimeSpan elapsedSinceTakeOff;
        private TimeSpan elapsedSinceDown;
        private bool isTapDown;
        private int maxFingerTouchedCount;

        /// <summary>
        /// Create a default Tap gesture configuration for single touch and single finger detection.
        /// </summary>
        public TapGesture()
            : this(1, 1)
        {
        }

        /// <summary>
        /// Create a default Tap gesture configuration for the given numbers of touches and fingers.
        /// </summary>
        /// <param name="tapCount">The number of taps required</param>
        /// <param name="fingerCount">The number of fingers required</param>
        public TapGesture(int tapCount, int fingerCount)
        {
            RequiredTapCount = tapCount;
            RequiredFingerCount = fingerCount;

            MaximumTimeBetweenTaps = TimeSpan.FromMilliseconds(400);
            MaximumDistanceTaps = 0.04f;
            MaximumPressTime = TimeSpan.FromMilliseconds(100);
        }

        /// <summary>
        /// Raised when a tap event is triggered
        /// </summary>
        public event EventHandler<TapEventArgs> Tap;

        public override void PreUpdate(TimeSpan elapsedTime)
        {
            base.PreUpdate(elapsedTime);

            if (isTapDown)
                elapsedSinceDown += DeltaTime;
            else
                elapsedSinceTakeOff += DeltaTime;
        }

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            // examine the tap gesture times and determine if the gesture has ended.
            if (HasGestureStarted && (elapsedSinceDown > MaximumPressTime || elapsedSinceTakeOff > MaximumTimeBetweenTaps))
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

            maxFingerTouchedCount = Math.Max(maxFingerTouchedCount, CurrentFingerCount);

            isTapDown = true;

            if (CurrentFingerCount == 1)
            {
                elapsedSinceTakeOff = TimeSpan.Zero;
                elapsedSinceDown = TimeSpan.Zero;
                HasGestureStarted = true;
            }

            if (HasGestureStarted && maxFingerTouchedCount > RequiredFingerCount)
                EndCurrentTap();

            if (HasGestureStarted && !HadFingerAtThatPosition(pos))
            {
                EndCurrentTap();

                maxFingerTouchedCount = CurrentFingerCount;
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
                if ((pos - beginPos).Length() < MaximumDistanceTaps)
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
                // Only process if a finger is down
                if (!FingerIdsToLastPos.ContainsKey(id))
                    continue;

                if ((fingerIdsToMovePos[id] - FingerIdsToLastPos[id]).Length() > MaximumDistanceTaps)
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

            if (CurrentFingerCount == 0)
            {
                elapsedSinceTakeOff = TimeSpan.Zero;
                isTapDown = false;

                if (HasGestureStarted && maxFingerTouchedCount == RequiredFingerCount)
                    ++currentTapCount;

                maxFingerTouchedCount = 0;
            }
        }

        private void EndCurrentTap()
        {
            // add the gesture to the tap event list if the number of tap requirement is fulfilled
            if (currentTapCount == RequiredTapCount)
            {
                var tapMeanPosition = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                var args = new TapEventArgs(PointerDevice, ElapsedSinceBeginning, RequiredFingerCount, currentTapCount, NormalizeVector(tapMeanPosition));
                Tap?.Invoke(this, args);
                SendChangedEvent(args);
            }

            currentTapCount = 0;

            HasGestureStarted = false;
            FingerIdToBeginPositions.Clear();
        }
    }
}