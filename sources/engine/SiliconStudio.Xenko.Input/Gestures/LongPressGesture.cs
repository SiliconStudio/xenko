// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A recognizer for a long press gesture.
    /// </summary>
    /// <remarks>A long press gesture can be composed of 1 or more fingers.</remarks>
    public class LongPressGesture : PointerGestureBase
    {
        private float maximumTransDst;
        private TimeSpan requiredPressTime;
        
        /// <summary>
        /// Create a default LongPress gesture configuration. 
        /// </summary>
        /// <remarks>Single finger and 1 second long press.</remarks>
        public LongPressGesture()
        {
            RequiredFingerCount = 1;
            RequiredPressTime = TimeSpan.FromSeconds(0.75);
            MaximumTranslationDistance = 0.02f;
        }

        /// <summary>
        /// The value represents the maximum distance a finger can translate during the longPress action.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        /// <remarks>
        /// By increasing this value, the user allows small movements of the fingers during the long press.
        /// By decreasing this value, the user forbids any movements during the long press.</remarks>
        public float MaximumTranslationDistance
        {
            get { return maximumTransDst; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                maximumTransDst = value;
            }
        }

        /// <summary>
        /// The time the user has to hold his finger on the screen to trigger the gesture.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public TimeSpan RequiredPressTime
        {
            get { return requiredPressTime; }
            set
            {
                requiredPressTime = value;
            }
        }

        protected override int CurrentFingerCount => FingerIdToBeginPositions.Count;

        /// <summary>
        /// Raised when a long press occured that matches this gesture
        /// </summary>
        public event EventHandler<LongPressEventArgs> LongPress;

        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (HasGestureStarted && ElapsedSinceBeginning >= RequiredPressTime)
            {
                var avgPosition = ComputeMeanPosition(FingerIdToBeginPositions.Values);
                var args = new LongPressEventArgs(PointerDevice, RequiredFingerCount, ElapsedSinceBeginning, NormalizeVector(avgPosition));
                LongPress?.Invoke(this, args);
                SendChangedEvent(args);
                HasGestureStarted = false;
            }
        }

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions[id] = pos;
            HasGestureStarted = (CurrentFingerCount == RequiredFingerCount);
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            foreach (var id in fingerIdsToMovePos.Keys)
            {
                // Only process if a finger is down
                if (!FingerIdToBeginPositions.ContainsKey(id))
                    continue;

                var dist = (fingerIdsToMovePos[id] - FingerIdToBeginPositions[id]).Length();
                if (dist > MaximumTranslationDistance)
                    HasGestureStarted = false;
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions.Remove(id);
            HasGestureStarted = (CurrentFingerCount == RequiredFingerCount);
        }
    }
}