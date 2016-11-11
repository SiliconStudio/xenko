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
        private float lastValue;
        private bool lastRelative;

        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        public event EventHandler<ChangedEventArgs> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        public event EventHandler<ChangedEventArgs> OnChanged;
        
        /// <summary>
        /// The last value of this action
        /// </summary>
        [DataMemberIgnore]
        public float Value => lastValue;

        public override void Update()
        {
            float newValue = 0.0f;
            bool relative = false;
            foreach (var gesture in Gestures.OfType<IAxisGesture>())
            {
                float v = gesture.Axis;
                if (Math.Abs(v) > Math.Abs(newValue))
                {
                    newValue = v;
                    relative = gesture.IsRelative;
                }
            }

            if (lastValue != newValue)
            {
                lastValue = newValue;
                lastRelative = relative;
                OnChanged?.Invoke(this, new ChangedEventArgs { Value = lastValue, Relative = lastRelative});
            }
            if (lastValue != 0.0f)
            {
                OnNotZero?.Invoke(this, new ChangedEventArgs { Value = lastValue, Relative = lastRelative });
            }
        }

        public override string ToString()
        {
            return $"Axis Action \"{MappingName}\", {nameof(Value)}: {Value}";
        }

        public class ChangedEventArgs : EventArgs
        {
            public float Value;
            public bool Relative;
        }
    }
}