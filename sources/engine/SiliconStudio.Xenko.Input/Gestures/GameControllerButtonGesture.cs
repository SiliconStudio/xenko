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
        public GameControllerButtonGesture()
        {
        }

        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex { get; set; }

        /// <summary>
        /// Id of the controller to watch
        /// </summary>
        public Guid ControllerId { get; set; }

        public GameControllerButtonGesture(int buttonIndex, Guid controllerId)
        {
            ButtonIndex = buttonIndex;
            ControllerId = controllerId;
        }

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.GameController.Id == ControllerId || ControllerId == Guid.Empty)
            {
                if (inputEvent.Index == ButtonIndex)
                    UpdateButton(inputEvent.State, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}, {nameof(ControllerId)}: {ControllerId}, {base.ToString()}";
        }

        protected bool Equals(GameControllerButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex && ControllerId.Equals(other.ControllerId);
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
                return (ButtonIndex * 397) ^ ControllerId.GetHashCode();
            }
        }
    }
}