// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Event class for the Composite gesture.
    /// </summary>
    public sealed class GestureEventComposite : GestureEvent
    {
        /// <summary>
        /// The position of the center of the composite transformation at the beginning of the gesture (in normalized coordinates [0,1]).
        /// </summary>
        /// <remarks>The center of the transformation corresponds to the middle of the 2 fingers.</remarks>
        public Vector2 CenterBeginningPosition { get; internal set; }

        /// <summary>
        /// The current position of the center of the composite transformation (in normalized coordinates [0,1]).
        /// </summary>
        /// <remarks>The center of the transformation corresponds to the middle of the 2 fingers.</remarks>
        public Vector2 CenterCurrentPosition { get; internal set; }

        /// <summary>
        /// The rotation angle (in radian) since the last event of the gesture.
        /// </summary>
        public float DeltaRotation { get; internal set; }

        /// <summary>
        /// The rotation angle (in radian) since the beginning of the gesture.
        /// </summary>
        public float TotalRotation { get; internal set; }

        /// <summary>
        /// The difference of scale since the last event of the gesture. 
        /// </summary>
        public float DeltaScale { get; internal set; }

        /// <summary>
        /// The difference of scale since the beginning of the gesture.
        /// </summary>
        public float TotalScale { get; internal set; }

        /// <summary>
        /// The translation (in pixels) performed since the last event of the gesture.
        /// </summary>
        public Vector2 DeltaTranslation { get; internal set; }

        /// <summary>
        /// The translation (in pixels) performed since the beginning of the gesture.
        /// </summary>
        public Vector2 TotalTranslation { get; internal set; }

        internal GestureEventComposite(GestureState state, TimeSpan deltaTime, TimeSpan totalTime, float deltaAngle, float totalAngle, 
            float deltaScale, float totalScale, Vector2 firstCenter, Vector2 lastCenter, Vector2 currentCenter)
        {
            Type = GestureType.Composite;
            State = state;
            DeltaTime = deltaTime;
            TotalTime = totalTime;
            DeltaRotation = deltaAngle;
            TotalRotation = totalAngle;
            DeltaScale = deltaScale;
            TotalScale = totalScale;
            DeltaTranslation = currentCenter-lastCenter;
            TotalTranslation = currentCenter-firstCenter;
            CenterBeginningPosition = firstCenter;
            CenterCurrentPosition = currentCenter;
        }
    }
}
