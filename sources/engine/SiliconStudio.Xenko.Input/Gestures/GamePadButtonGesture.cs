// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    public class GamePadButtonGesture : ButtonGestureBase, IInputEventListener<GamePadButtonEvent>
    {
        /// <summary>
        /// The gamepad button identifier
        /// </summary>
        public GamePadButton GamePadButton;

        public GamePadButtonGesture()
        {
        }

        public GamePadButtonGesture(GamePadButton button)
        {
            GamePadButton = button;
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (GamePadButton == inputEvent.Button)
                UpdateButton(inputEvent.State, inputEvent.Device);
        }

        public override string ToString()
        {
            return $"{nameof(GamePadButton)}: {GamePadButton}";
        }

        protected bool Equals(GamePadButtonGesture other)
        {
            return GamePadButton == other.GamePadButton;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return GamePadButton.GetHashCode();
        }
    }
}