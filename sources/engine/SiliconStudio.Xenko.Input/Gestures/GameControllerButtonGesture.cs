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
    [Display("Controller Button")]
    public class GameControllerButtonGesture : ButtonGestureBase, IInputEventListener<GameControllerButtonEvent>, IGameControllerGesture
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex = 0;

        /// <summary>
        /// Id of the controller to watch
        /// </summary>
        private Guid controllerId;

        public GameControllerButtonGesture()
        {
        }

        public GameControllerButtonGesture(int buttonIndex, Guid controllerId)
        {
            ButtonIndex = buttonIndex;
            this.controllerId = controllerId;
        }

        /// <summary>
        /// Id of the controller to watch
        /// </summary>
        public Guid ControllerId
        {
            get { return controllerId; }
            set { controllerId = value; }
        }

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.GameController.Id == controllerId || controllerId == Guid.Empty)
            {
                if (inputEvent.Index == ButtonIndex)
                    UpdateButton(inputEvent.State, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}, {nameof(controllerId)}: {controllerId}, {base.ToString()}";
        }

        protected bool Equals(GameControllerButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex && controllerId.Equals(other.controllerId);
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
            unchecked
            {
                return (ButtonIndex * 397) ^ controllerId.GetHashCode();
            }
        }
    }
}