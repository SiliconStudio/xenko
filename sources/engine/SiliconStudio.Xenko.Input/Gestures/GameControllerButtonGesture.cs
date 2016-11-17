// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A button gesture generated from a game controller button press
    /// </summary>
    [DataContract]
    public class GameControllerButtonGesture : ButtonGestureBase, IInputEventListener<GameControllerButtonEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex = 0;

        public GameControllerButtonGesture()
        {
        }

        public GameControllerButtonGesture(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.Index == ButtonIndex)
                UpdateButton(inputEvent.State, inputEvent.Device);
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}";
        }

        protected bool Equals(GameControllerButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameControllerButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return ButtonIndex;
        }
    }
}