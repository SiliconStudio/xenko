// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    public enum GamePadButtonType
    {
        PushButton,
        ToggleButton
    }

    public enum GamePadAxisType
    {
        RelativeAxis,
        AbsoluteAxis
    }

    public class GamePadObjectInfo
    {
        public string Name;
        public int Index;
        public override string ToString()
        {
            return $"GamePad Object {Index} {{{Name}}}";
        }
    }

    /// <summary>
    /// Provides extra information about a gamepad button
    /// </summary>
    public class GamePadButtonInfo : GamePadObjectInfo
    {
        public GamePadButtonType Type;
        public override string ToString()
        {
            return $"GamePad Button {Index} {{{Name}}} [{Type}]";
        }
    }

    /// <summary>
    /// Provides extra information about a gamepad axis
    /// </summary>
    public class GamePadAxisInfo : GamePadObjectInfo
    {
        public GamePadAxisType Type;
        public override string ToString()
        {
            return $"GamePad Axis {Index} {{{Name}}} [{Type}]";
        }
    }


    /// <summary>
    /// Provides extra information about a gamepad POV controller
    /// </summary>
    public class GamePadPovControllerInfo : GamePadObjectInfo
    {
        public override string ToString()
        {
            return $"GamePad PovController {Index} {{{Name}}}";
        }
    }

    public enum GamePadButtonState
    {
        Pressed,
        Released
    }

    public class GamePadButtonEvent : EventArgs
    {
        public int Index;
        public GamePadButtonState State;
    }

    public class GamePadAxisEvent : EventArgs
    {
        public int Index;
        public float Value;
    }

    public interface IGamePadDevice : IInputDevice
    {
        /// <summary>
        /// The index of the GamePad
        /// </summary>
        int Index { get; }

        /// <summary>
        /// <c>true</c> if the GamePad is currently connected
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Information about the buttons on this GamePad
        /// </summary>
        IReadOnlyCollection<GamePadButtonInfo> ButtonInfos { get; }
        /// <summary>
        /// Information about the axes on this GamePad
        /// </summary>
        IReadOnlyCollection<GamePadAxisInfo> AxisInfos { get; }
        /// <summary>
        /// Information about the point of view controllers (dpad) on this GamePad 
        /// </summary>
        IReadOnlyCollection<GamePadPovControllerInfo> PovControllerInfos { get; }

        /// <summary>
        /// Raised when a button on the GamePad changes state
        /// </summary>
        EventHandler<GamePadButtonEvent> OnButton { get; set; }

        /// <summary>
        /// Raised when an axis on the GamePad changes state
        /// </summary>
        EventHandler<GamePadAxisEvent> OnAxisChanged { get; set; }

        /// <summary>
        /// Raised when a point of view controller on the GamePad changes state
        /// </summary>
        EventHandler<GamePadAxisEvent> OnPovControllerChanged { get; set; }

        /// <summary>
        /// Retrieves the button state of a single button
        /// </summary>
        /// <param name="index">The button's index, as exposed in <see cref="ButtonInfos"/></param>
        /// <returns><c>true</c> if the button is pressed, <c>false</c> otherwise</returns>
        bool GetButton(int index);

        /// <summary>
        /// Retrieves the state of a single axis
        /// </summary>
        /// <param name="index">The axis' index, as exposed in <see cref="AxisInfos"/></param>
        /// <returns>The value read directly from the axis</returns>
        float GetAxis(int index);

        /// <summary>
        /// Retrieves the state of a single point of view controller
        /// </summary>
        /// <param name="index">The controller's index, as exposed in <see cref="PovControllerInfos"/></param>
        /// <returns>a value from 0 to 1(excluded) indicating a direction starting from up going clockwise</returns>
        float GetPovController(int index);
    }
}