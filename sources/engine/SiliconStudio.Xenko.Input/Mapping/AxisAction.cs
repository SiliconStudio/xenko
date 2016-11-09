// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    [DataContract]
    public class AxisAction : InputAction
    {
        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        public event EventHandler<ChangedEventArgs> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        public event EventHandler<ChangedEventArgs> OnChanged;

        private float lastValue;

        /// <summary>
        /// The last value of this action
        /// </summary>
        [DataMemberIgnore]
        public float Value => lastValue;

        public override void Update()
        {
            float newValue = 0.0f;
            foreach (var gesture in Gestures.OfType<IAxisGesture>())
            {
                float v = gesture.Axis;

                // Apply scalable properties
                var scalable = gesture as ScalableInputGesture;
                if (scalable != null)
                    v = (scalable.Inverted ? -v : v) * scalable.Sensitivity;

                if (Math.Abs(v) > Math.Abs(newValue))
                    newValue = v;
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