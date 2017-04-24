// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    internal sealed class GestureRecognizerComposite : GestureRecognizerContMotion
    {
        private GestureConfigComposite ConfigComposite { get { return (GestureConfigComposite)Config; } }
        
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

        public GestureRecognizerComposite(GestureConfigComposite config, float screenRatio)
            : base(config, screenRatio)
        {
        }

        protected override void InitializeGestureVariables()
        {
            // initialize first and second finger ids
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

        protected override bool GestureBeginningConditionFulfilled()
        {
            return Math.Abs(currentRotation) >= ConfigComposite.MinimumRotationAngle
                || currentScale <= ConfigComposite.MinimumScaleValueInv || currentScale >= ConfigComposite.MinimumScaleValue
                || (currentCenter - beginCenter).Length() >= ConfigComposite.MinimumTranslationDistance;
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
            currentScale = Math.Abs(beginVectorLength) > MathUtil.ZeroTolerance? currentVector.Length() / beginVectorLength: 0;
        }

        protected override void AddGestureEventToCurrentList(GestureState state)
        {
            var deltaRotation = currentRotation - lastRotation;
            var deltaScale = currentScale - lastScale;
            CurrentGestureEvents.Add(new GestureEventComposite(state, ElapsedSinceLast, ElapsedSinceBeginning, deltaRotation, currentRotation, deltaScale, currentScale,
                                                               NormalizeVector(beginCenter), NormalizeVector(lastCenter), NormalizeVector(currentCenter)));

            lastRotation = currentRotation;
            lastScale = currentScale;
            lastCenter = currentCenter;

            base.AddGestureEventToCurrentList(state);
        }
    }
}
