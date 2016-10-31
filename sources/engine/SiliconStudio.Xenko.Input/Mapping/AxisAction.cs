// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    public class AxisAction : InputAction
    {
        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        event EventHandler<ChangedEventArgs> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        event EventHandler<ChangedEventArgs> OnChanged;

        private float lastValue;

        /// <summary>
        /// The last value of this action
        /// </summary>
        public float Value => lastValue;

        public override void Update()
        {
            float newValue = 0.0f;
            foreach (var gesture in Gestures.OfType<IAxisGesture>())
            {
                if (Math.Abs(gesture.Axis) > Math.Abs(newValue))
                    newValue = gesture.Axis;
            }

            if (lastValue != newValue)
            {
                lastValue = newValue;
                OnChanged?.Invoke(this, new ChangedEventArgs { Value = lastValue });
            }
            if (lastValue != 0.0f)
            {
                OnNotZero?.Invoke(this, new ChangedEventArgs { Value = lastValue });
            }
        }

        public override string ToString()
        {
            return $"Axis Action \"{MappingName}\", {nameof(Value)}: {Value}";
        }

        public class ChangedEventArgs : EventArgs
        {
            public float Value;
        }
    }
}