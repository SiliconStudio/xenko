// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    public class DragGesture : ContinuousMotionGesture
    {
        private Dictionary<int, Vector2> fingerIdsToLowFilteredPos = new Dictionary<int, Vector2>();

        private Vector2 startPosition;
        private Vector2 lastPosition;
        private Vector2 currPosition;

        private float minimumDragDistance;
        private Vector2 allowedErrorMargins;
        private GestureShape dragShape;

        /// <summary>
        /// Create a default drag gesture configuration for one finger free dragging.
        /// </summary>
        public DragGesture()
            : this(GestureShape.Free)
        {
        }

        /// <summary> 
        /// Create a default drag gesture configuration for one finger dragging.
        /// </summary>
        /// <param name="dragShape">The dragging shape</param>
        public DragGesture(GestureShape dragShape)
        {
            DragShape = dragShape;
            RequiredFingerCount = 1;
            AllowedErrorMargins = 0.02f * Vector2.One;
            MinimumDragDistance = 0.02f;
        }

        /// <summary>
        /// Specify the minimum translation distance required  before that the gesture can be recognized as a Drag.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided value was negative.</exception>
        /// <remarks>The user can reduce this value if he needs the drag gesture to be triggered even for very small drags.
        /// On the contrary, he can increase this value if he wants to avoid to deals with too small drags.</remarks>
        public float MinimumDragDistance
        {
            get { return minimumDragDistance; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                minimumDragDistance = value;
            }
        }

        /// <summary>
        /// The (x,y) error margins allowed during directional dragging.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The provided x or y value was not positive.</exception>
        /// <remarks>Those values are used only for directional (vertical or horizontal) dragging. 
        /// Decrease those values to trigger the gesture only when the dragging is perfectly in the desired direction.
        /// Increase those values to allow directional gestures to be more approximative.</remarks>
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
        /// The shape (direction) of the drag gesture.
        /// </summary>
        /// <exception cref="InvalidOperationException">Tried to modify the configuration after it has been frozen by the system.</exception>
        public GestureShape DragShape
        {
            get { return dragShape; }
            set
            {
                dragShape = value;
            }
        }

        /// <summary>
        /// Raised when a new composite gesture has began/ened or it's values changed
        /// </summary>
        public event EventHandler<DragEventArgs> Drag;

        protected override void InitializeGestureVariables()
        {
            startPosition = ComputeMeanPosition(FingerIdsToLastPos.Values);
            lastPosition = startPosition;
            fingerIdsToLowFilteredPos = new Dictionary<int, Vector2>(FingerIdsToLastPos);
        }

        protected override void UpdateGestureVarsAndPerfomChecks()
        {
            foreach (var id in FingerIdsToLastPos.Keys)
            {
                // check that the drag shape is respected and end the gesture if it is not the case
                if (DragShape != GestureShape.Free)
                {
                    var compIndex = DragShape == GestureShape.Horizontal ? 1 : 0;
                    if (Math.Abs(FingerIdsToLastPos[id][compIndex] - fingerIdsToLowFilteredPos[id][compIndex]) > AllowedErrorMargins[compIndex])
                        HasGestureStarted = false;
                }

                // update the finger low filtered position for the finger
                const float lowFilterCoef = 0.9f;
                fingerIdsToLowFilteredPos[id] = fingerIdsToLowFilteredPos[id] * lowFilterCoef + (1f - lowFilterCoef) * FingerIdsToLastPos[id];
            }

            currPosition = ComputeMeanPosition(FingerIdsToLastPos.Values);
        }

        protected override bool GestureBeginningConditionFulfilled()
        {
            return (currPosition - startPosition).Length() >= MinimumDragDistance;
        }

        protected override void AddGestureEventToCurrentList(PointerGestureEventType eventType)
        {
            var deltaTrans = currPosition - lastPosition;
            var args = new DragEventArgs(PointerDevice, eventType, RequiredFingerCount, ElapsedSinceLast, ElapsedSinceBeginning, DragShape,
                NormalizeVector(startPosition), NormalizeVector(currPosition), NormalizeVector(deltaTrans));
            Drag?.Invoke(this, args);
            SendChangedEvent(args);

            lastPosition = currPosition;

            base.AddGestureEventToCurrentList(eventType);
        }
    }
}