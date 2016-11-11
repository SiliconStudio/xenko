// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that triggers on a certain condition
    /// </summary>
    [DataContract]
    public class ButtonAction : InputAction
    {
        /// <summary>
        /// Raised when the action was trigerred
        /// </summary>
        public event EventHandler<ChangedEventArgs> OnChanged;

        private bool lastValue;

        /// <summary>
        /// The last value of the button action
        /// </summary>
        [DataMemberIgnore]
        public bool Value => lastValue;

        public override void Update()
        {
            bool newValue = false;
            foreach (var gesture in Gestures.OfType<IButtonGesture>())
            {
                newValue = newValue || gesture.Button;
            }
            if (lastValue != newValue)
            {
                OnChanged?.Invoke(this, new ChangedEventArgs { Value = Value });
                lastValue = newValue;
            }
        }

        public override string ToString()
        {
            return $"Button Action \"{MappingName}\", {nameof(Value)}: {Value}";
        }

        public class ChangedEventArgs : EventArgs
        {
            public IButtonGesture Gesture;
            public bool Value;
            public ButtonState State => Value ? ButtonState.Pressed : ButtonState.Released;
        }
    }
}