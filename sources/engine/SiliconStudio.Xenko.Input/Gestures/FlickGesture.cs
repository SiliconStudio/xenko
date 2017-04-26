// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary> 
    /// Recognizer for a flick gesture.
    /// </summary>
    /// <remarks>A Flick gesture can be composed of 1 or more fingers.</remarks>
    public class FlickGesture : PointerGestureBase
    {
        private Vector2 allowedErrorMargins;
        private GestureShape flickShape;
        private float minimumAverageSpeed;
        private float minimumFlickLength;

        /// <summary>
        /// Create a default Flick gesture configuration for one finger free flicking.
        /// </summary>
        public FlickGesture()
            : this(GestureShape.Free)
        {
        }

        /// <summary>
        /// Create a default gesture configuration for one finger flicking.
        /// </summary>
        /// <param name="flickShape">The shape of the flicking.</param>
        public FlickGesture(GestureShape flickShape)
        {
            FlickShape = flickShape;
            RequiredFingerCount = 1;
            MinimumAverageSpeed = 0.4f;
            MinimumFlickLength = 0.04f;
            AllowedErrorMargins = 0.02f * Vector2.One;
        }
        
        /// <summary>
        /// The (x,y) error margins allowed during directional dragging.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided x or y value was not positive.</exception>
        public Vector2 AllowedErrorMargins
        {
            get { return allowedErrorMargins; }
            set
            {
                if (value.X < 0 || value.Y < 0)
                    throw new ArgumentOutOfRangeException("value");

                allowedErrorMargins = value;
            }
        }

        /// <summary>
        /// The shape of the flick gesture.
        /// </summary>        
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public GestureShape FlickShape
        {
            get { return flickShape; }
            set
            {
                flickShape = value;
            }
        }

        /// <summary>
        /// The minimum average speed of the gesture to be detected as a flick.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be positive</exception>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public float MinimumAverageSpeed
        {
            get { return minimumAverageSpeed; }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                minimumAverageSpeed = value;
            }
        }


        /// <summary>
        /// The minimum distance that the flick gesture has to cross from its origin to be detected has Flick.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public float MinimumFlickLength
        {
            get { return minimumFlickLength; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                minimumFlickLength = value;
            }
        }

        /// <summary>
        /// Raised when a new flick gesture occured
        /// </summary>
        public event EventHandler<FlickEventArgs> Flick;

        protected override void ProcessDownEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions[id] = pos;
            FingerIdsToLastPos[id] = pos;
            HasGestureStarted = (CurrentFingerCount == RequiredFingerCount);
        }

        protected override void ProcessMoveEventPointers(Dictionary<int, Vector2> fingerIdsToMovePos)
        {
            if (!HasGestureStarted)
                return;

            foreach (var id in fingerIdsToMovePos.Keys)
            {
                var newPos = fingerIdsToMovePos[id];

                // check that the shape of the flick is respected, stop the gesture if it is not the case
                if (FlickShape != GestureShape.Free)
                {
                    var compIndex = FlickShape == GestureShape.Horizontal ? 1 : 0;
                    if (Math.Abs(newPos[compIndex] - FingerIdToBeginPositions[id][compIndex]) > AllowedErrorMargins[compIndex])
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
                if (translDist > MinimumFlickLength && translDist / ElapsedSinceBeginning.TotalSeconds > MinimumAverageSpeed)
                {
                    var args = new FlickEventArgs(PointerDevice, RequiredFingerCount, ElapsedSinceBeginning, FlickShape, NormalizeVector(startPos), NormalizeVector(currPos));
                    Flick?.Invoke(this, args);
                    SendChangedEvent(args);
                    HasGestureStarted = false;
                }
            }
        }

        protected override void ProcessUpEventPointer(int id, Vector2 pos)
        {
            FingerIdToBeginPositions.Remove(id);
            FingerIdsToLastPos.Remove(id);
            HasGestureStarted = (CurrentFingerCount == RequiredFingerCount);
        }
    }
}