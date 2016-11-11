// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Data.Common;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Extends InputGesture to have a setting to adjust the <see cref="Sensitivity"/> and toggle <see cref="Inverted"/>
    /// </summary>
    [DataContract]
    public class ScalableInputGesture : InputGesture
    {
        /// <summary>
        /// The multiplier applied to the output axis or direction value of this gesture
        /// </summary>
        public float Sensitivity = 1.0f;

        /// <summary>
        /// Should the axis or direction be inverted
        /// </summary>
        public bool Inverted = false;

        /// <summary>
        /// Returns the input value with <see cref="Inverted"/> and <see cref="Sensitivity"/> applied to it
        /// </summary>
        /// <param name="v">the input axis value</param>
        /// <returns>The scaled output value</returns>
        protected float GetScaledOutput(float v)
        {
            return v = (Inverted ? -v : v) * Sensitivity;
        }

        /// <summary>
        /// Returns the input direction with <see cref="Inverted"/> and <see cref="Sensitivity"/> applied to it
        /// </summary>
        /// <remarks>Inversion inverts both axes</remarks>
        /// <param name="v">the input direction value</param>
        /// <returns>The scaled output value</returns>
        protected Vector2 GetScaledOutput(Vector2 v)
        {
            return v = (Inverted ? -v : v) * Sensitivity;
        }
    }
}