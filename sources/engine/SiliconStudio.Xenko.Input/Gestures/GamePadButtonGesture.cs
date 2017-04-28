// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    [Display("Gamepad Button")]
    public class GamePadButtonGesture : ButtonGestureBase, IInputEventListener<GamePadButtonEvent>, IGamePadGesture
    {
        /// <summary>
        /// The gamepad button identifier
        /// </summary>
        public GamePadButton GamePadButton;

        private int gamePadIndex;

        public GamePadButtonGesture()
        {
        }

        public GamePadButtonGesture(GamePadButton button, int gamePadIndex)
        {
            GamePadButton = button;
            this.gamePadIndex = gamePadIndex;
        }

        /// <summary>
        /// The index of the gamepad to watch
        /// </summary>
        public int GamePadIndex
        {
            get { return gamePadIndex; }
            set { gamePadIndex = value; }
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == gamePadIndex)
            {
                if (GamePadButton == inputEvent.Button)
                    UpdateButton(inputEvent.State, inputEvent.Device);
            }
        }

        public override string ToString()
        {
            return $"{nameof(GamePadButton)}: {GamePadButton}, {nameof(gamePadIndex)}: {gamePadIndex}, {base.ToString()}";
        }

        protected bool Equals(GamePadButtonGesture other)
        {
            return GamePadButton == other.GamePadButton && gamePadIndex == other.gamePadIndex;
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
            unchecked
            {
                return ((int)GamePadButton * 397) ^ gamePadIndex;
            }
        }
    }
}