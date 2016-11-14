// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a direction and velocity
    /// </summary>
    [DataContract]
    public class DirectionAction : InputAction
    {
        private Vector2 lastValue;
        private bool lastRelative;

        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        public event EventHandler<ChangedEventArgs> NotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        public event EventHandler<ChangedEventArgs> Changed;

        /// <summary>
        /// The last value of this action
        /// </summary>
        [DataMemberIgnore]
        public Vector2 Value => lastValue;
        
        public override void Update()
        {
            Vector2 target = Vector2.Zero;
            float largest = 0.0f;
            bool relative = false;
            foreach (var gesture in Gestures.OfType<IDirectionGesture>())
            {
                Vector2 v = gesture.Direction;
                float length = v.Length();
                if (length > largest)
                {
                    target = gesture.Direction;
                    relative = gesture.IsRelative;
                    largest = length;
                }
            }

            if (lastValue != target)
            {
                lastValue = target;
                lastRelative = relative;
                Changed?.Invoke(this, new ChangedEventArgs { Value = Value, Relative = lastRelative });
            }
            if (largest > 0)
            {
                NotZero?.Invoke(this, new ChangedEventArgs { Value = Value, Relative = lastRelative });
            }
        }

        public override string ToString()
        {
            return $"Direction Action \"{MappingName}\", {nameof(Value)}: {Value}";
        }

        public class ChangedEventArgs : EventArgs
        {
            public Vector2 Value;
            public bool Relative;
        }
    }
}