// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Describes the state of a gamepad.
    /// </summary>
    /// <seealso cref="InputManager.GetGamePad"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct GamePadState : IEquatable<GamePadState>
    {
        /// <summary>
        /// Bitmask of the gamepad buttons.
        /// </summary>
        public GamePadButton Buttons;

        /// <summary>
        /// Left thumbstick x-axis/y-axis value. The value is in the range [-1.0f, 1.0f] for both axis.
        /// </summary>
        public Vector2 LeftThumb;

        /// <summary>
        /// Right thumbstick x-axis/y-axis value. The value is in the range [-1.0f, 1.0f] for both axis.
        /// </summary>
        public Vector2 RightThumb;

        /// <summary>
        /// The left trigger analog control in the range [0, 1.0f]. See remarks.
        /// </summary>
        /// <remarks>
        /// Some controllers are not supporting the range of value and may act as a simple button returning only 0 or 1.
        /// </remarks>
        public float LeftTrigger;

        /// <summary>
        /// The right trigger analog control in the range [0, 1.0f]. See remarks.
        /// </summary>
        /// <remarks>
        /// Some controllers are not supporting the range of value and may act as a simple button returning only 0 or 1.
        /// </remarks>
        public float RightTrigger;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(GamePadState other)
        {
            return Buttons.Equals(other.Buttons) && LeftThumb.Equals(other.LeftThumb) && RightThumb.Equals(other.RightThumb) && LeftTrigger.Equals(other.LeftTrigger) && RightTrigger.Equals(other.RightTrigger);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GamePadState && Equals((GamePadState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Buttons.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftThumb.GetHashCode();
                hashCode = (hashCode * 397) ^ RightThumb.GetHashCode();
                hashCode = (hashCode * 397) ^ LeftTrigger.GetHashCode();
                hashCode = (hashCode * 397) ^ RightTrigger.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left gamepad value.</param>
        /// <param name="right">The right gamepad value.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GamePadState left, GamePadState right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left gamepad value.</param>
        /// <param name="right">The right gamepad value.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GamePadState left, GamePadState right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Buttons: {Buttons}, LeftThumb: {LeftThumb}, RightThumb: {RightThumb}, LeftTrigger: {LeftTrigger}, RightTrigger: {RightTrigger}";
        }

        public void Update(GamePadPovControllerEvent povEvent)
        {
            // Check if this maps to DPAD
            if (povEvent.Button == GamePadButton.Pad)
            {
                Buttons &= ~GamePadButton.Pad;// Clear old DPAD value
                if (povEvent.Enabled)
                {
                    Buttons |= GamePadUtils.PovControllerToButton(povEvent.Value);
                }
            }
        }
        public void Update(GamePadButtonEvent buttonEvent)
        {
            if (buttonEvent.State == ButtonState.Pressed)
                Buttons |= buttonEvent.Button; // Set bits
            else
                Buttons &= ~buttonEvent.Button; // Clear bits
        }
        public void Update(GamePadAxisEvent axisEvent)
        {
            switch (axisEvent.Axis)
            {
                case GamePadAxis.LeftThumbX:
                    LeftThumb.X = axisEvent.MappedValue;
                    break;
                case GamePadAxis.LeftThumbY:
                    LeftThumb.Y = axisEvent.MappedValue;
                    break;
                case GamePadAxis.RightThumbX:
                    RightThumb.X = axisEvent.MappedValue;
                    break;
                case GamePadAxis.RightThumbY:
                    RightThumb.Y = axisEvent.MappedValue;
                    break;
                case GamePadAxis.LeftTrigger:
                    LeftTrigger = axisEvent.MappedValue;
                    break;
                case GamePadAxis.RightTrigger:
                    RightTrigger = axisEvent.MappedValue;
                    break;
            }
        }
    }
}