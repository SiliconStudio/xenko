// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A recognizer for a Composite gesture.
    /// </summary>
    /// <remarks>
    /// <para>A composite gesture is a transformation which is a composition of a translation, a rotation and a scale.
    /// It is performed by using two fingers and performing translation, scale and rotation motions.</para>
    /// <para>A composite gesture can only be composed of 2 fingers. 
    /// Trying to modify the <see cref="PointerGesture.RequiredFingerCount"/> field will throw an exception.</para></remarks>
    public class CompositeGesture : ContinuousMotionGesture
    {
        private int firstFingerId;
        private int secondFingerId;

        private Vector2 beginVectorNormalized;
        private float beginVectorLength;
        private Vector2 beginCenter;
        private Vector2 lastCenter;
        private Vector2 currentCenter;
        private float currentRotation;
        private float lastRotation;
        private float lastScale;
        private float currentScale;

        private float minimumScaleValue;
        private float mminimumTranslationDistance;
        private float minimumRotationAngle;

        protected float MinimumScaleValueInv { get; set; }

        public CompositeGesture()
        {
            RestrictedFingerCount = 2;
            RequiredFingerCount = 2;
            MinimumRotationAngle = 0.1f;
            MinimumScaleValue = 1.075f;
            MinimumTranslationDistance = 0.016f;
        }

        /// <summary>
        /// The scale value above which the gesture is started.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be greater or equal to 1.</exception>
        /// <remarks>The user can increase this value if he has small or no interest in the scale component of the transformation. 
        /// By doing so, he avoids triggering the Composite Gesture when only small scale changes happen. 
        /// On the contrary, the user can decrease this value if he wants to be immediately warned about the smallest change in scale.</remarks>
        public float MinimumScaleValue
        {
            get { return minimumScaleValue; }
            set
            {
                if (value <= 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                minimumScaleValue = value;
                MinimumScaleValueInv = 1 / minimumScaleValue;
            }
        }

        /// <summary>
        /// The translation distance above which the gesture is started.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value has to be positive.</exception>
        /// <remarks>The user can increase this value if he has small or no interest in the translation component of the transformation. 
        /// By doing so, he avoids triggering the Composite Gesture when only small translation changes happen. 
        /// On the contrary, the user can decrease this value if he wants to be immediately warned about the smallest change in translation.</remarks>
        public float MinimumTranslationDistance
        {
            get { return mminimumTranslationDistance; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                mminimumTranslationDistance = value;
            }
        }


        /// <summary>
        /// The rotation angle (in radian) above which the gesture is started.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The angle has to be strictly positive.</exception>
        /// <remarks>The user can increase this value if he has small or no interest in the rotation component of the transformation. 
        /// By doing so, he avoids triggering the Composite Gesture when only small rotation changes happen. 
        /// On the contrary, the user can decrease this value if he wants to be immediately warned about the smallest change in rotation.</remarks>
        public float MinimumRotationAngle
        {
            get { return minimumRotationAngle; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                minimumRotationAngle = value;
            }
        }

        protected override void InitializeGestureVariables()
        {
            // initialize first and second finger ids
            // ReSharper disable once GenericEnumeratorNotDisposed
            var keysEnum = FingerIdsToLastPos.Keys.GetEnumerator();
            keysEnum.MoveNext(); firstFingerId = keysEnum.Current;
            keysEnum.MoveNext(); secondFingerId = keysEnum.Current;

            var beginDirVec = FingerIdsToLastPos[secondFingerId] - FingerIdsToLastPos[firstFingerId];
            beginVectorNormalized = Vector2.Normalize(beginDirVec);
            beginVectorLength = beginDirVec.Length();

            beginCenter = (FingerIdsToLastPos[secondFingerId] + FingerIdsToLastPos[firstFingerId]) / 2;
            lastCenter = beginCenter;
            lastRotation = 0;
            lastScale = 1;
        }

        /// <summary>
        /// Raised when a new composite gesture has began/ened or it's values changed
        /// </summary>
        public new event EventHandler<CompositeEventArgs> Changed;

        protected override bool GestureBeginningConditionFulfilled()
        {
            return Math.Abs(currentRotation) >= MinimumRotationAngle
                || currentScale <= MinimumScaleValueInv || currentScale >= MinimumScaleValue
                || (currentCenter - beginCenter).Length() >= MinimumTranslationDistance;
        }

        protected override void UpdateGestureVarsAndPerfomChecks()
        {
            var currentVector = FingerIdsToLastPos[secondFingerId] - FingerIdsToLastPos[firstFingerId];
            var currentVectorNormalized = Vector2.Normalize(currentVector);

            // Update the gesture current rotation value
            var rotSign = beginVectorNormalized[0] * currentVectorNormalized[1] - beginVectorNormalized[1] * currentVectorNormalized[0];
            currentRotation = Math.Sign(rotSign) * (float)Math.Acos(Vector2.Dot(currentVectorNormalized, beginVectorNormalized));

            // Update the gesture current center of transformation
            currentCenter = (FingerIdsToLastPos[secondFingerId] + FingerIdsToLastPos[firstFingerId]) / 2;

            // Update the gesture current scale
            currentScale = Math.Abs(beginVectorLength) > MathUtil.ZeroTolerance ? currentVector.Length() / beginVectorLength : 0;
        }

        protected override void AddGestureEventToCurrentList(PointerGestureEventType eventType)
        {
            var deltaRotation = currentRotation - lastRotation;
            var deltaScale = currentScale - lastScale;

            var args = new CompositeEventArgs(PointerDevice, eventType, ElapsedSinceLast, ElapsedSinceBeginning, deltaRotation, currentRotation,
                deltaScale, currentScale, NormalizeVector(beginCenter), NormalizeVector(lastCenter), NormalizeVector(currentCenter));

            Changed?.Invoke(this, args);
            SendChangedEvent(args);

            lastRotation = currentRotation;
            lastScale = currentScale;
            lastCenter = currentCenter;

            base.AddGestureEventToCurrentList(eventType);
        }
    }
}