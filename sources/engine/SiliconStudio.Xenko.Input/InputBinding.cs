// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows.Input;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Common data for all input bindings
    /// </summary>
    public class InputBinding
    {
        /// <summary>
        /// Sensitivity (scales the output)
        /// </summary>
        public readonly float Sensitivity;

        /// <summary>
        /// True for relative input devices, such as mouse
        /// </summary>
        public readonly bool IsRelative;

        /// <summary>
        /// True if input is inverted 1 -> -1
        /// </summary>
        public readonly bool IsInverted;

        /// <summary>
        /// The underlying virtual button
        /// </summary>
        public readonly IVirtualButton VirtualButton;

        public InputBinding(IVirtualButton virtualButton, float sensitivity = 1.0f, bool inverted = false)
        {
            this.VirtualButton = virtualButton;
            this.IsInverted = inverted;
            this.Sensitivity = sensitivity;

            // Only mouse is relative since it used MouseDelta
            var vb = virtualButton as VirtualButton;
            if (vb == null)
                throw new ArgumentException("Virtual button must be castable to VirtualButton");
            this.IsRelative = vb.Type == VirtualButtonType.Mouse;
        }
        public float GetValue(InputManager inputManager)
        {
            float val = VirtualButton.GetValue(inputManager);
            return IsInverted ? -val : val;
        }
    }
}