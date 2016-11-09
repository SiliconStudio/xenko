// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Extends InputGesture to have a setting to adjust the <see cref="Sensitivity"/> and toggle <see cref="Inverted"/>
    /// </summary>
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
    }
}