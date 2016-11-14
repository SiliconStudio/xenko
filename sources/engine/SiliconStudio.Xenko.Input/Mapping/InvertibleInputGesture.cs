// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Data.Common;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Extends InputGesture to have an <see cref="Inverted"/> setting
    /// </summary>
    [DataContract]
    public class InvertibleInputGesture : InputGesture
    {
        /// <summary>
        /// Should the axis or direction be inverted
        /// </summary>
        public bool Inverted = false;

        /// <summary>
        /// Returns the input value with <see cref="Inverted"/> applied to it
        /// </summary>
        /// <param name="v">the input axis value</param>
        /// <returns>The scaled output value</returns>
        protected float GetScaledOutput(float v)
        {
            return v = (Inverted ? -v : v);
        }

        /// <summary>
        /// Returns the input direction with <see cref="Inverted"/> applied to it
        /// </summary>
        /// <remarks>Inversion inverts both axes</remarks>
        /// <param name="v">the input direction value</param>
        /// <returns>The scaled output value</returns>
        protected Vector2 GetScaledOutput(Vector2 v)
        {
            return v = (Inverted ? -v : v);
        }
    }
}