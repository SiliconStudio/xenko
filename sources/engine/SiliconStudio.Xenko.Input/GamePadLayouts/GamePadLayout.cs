// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for a mapping of anonymous gamepad objects to a common controller layout, as used with <see cref="GamePadState"/>. Derive from this type to create custom layouts
    /// </summary>
    public abstract class GamePadLayout
    {
        /// <summary>
        /// Should pov controller 0 be mapped to the directional pad?
        /// </summary>
        protected bool MapFirstPovToPad = true;

        private List<GamePadButton> buttonMap = new List<GamePadButton>();
        private List<MappedAxis> axisMap = new List<MappedAxis>();

        /// <summary>
        /// Checks if a device matches this gamepad layout, and thus should use this when mapping it to a <see cref="GamePadState"/>
        /// </summary>
        /// <param name="device"></param>
        public abstract bool MatchDevice(IGamePadDevice device);

        /// <summary>
        /// Maps the raw gamepad event to a gamepad state event
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputEvent">The gamepad input event to adjust</param>
        public virtual void MapInputEvent(IGamePadDevice device, InputEvent inputEvent)
        {
            var buttonEvent = inputEvent as GamePadButtonEvent;
            if (buttonEvent != null)
            {
                if (buttonEvent.Index < buttonMap.Count)
                    buttonEvent.Button = buttonMap[buttonEvent.Index];
            }
            else
            {
                var axisEvent = inputEvent as GamePadAxisEvent;
                if (axisEvent != null)
                {
                    if (axisEvent.Index < axisMap.Count)
                    {
                        var mappedAxis = axisMap[axisEvent.Index];
                        axisEvent.Axis = mappedAxis.Axis;
                        axisEvent.MappedValue = mappedAxis.Invert ? -axisEvent.Value : axisEvent.Value;
                    }
                }
                else if(MapFirstPovToPad)
                {
                    var povEvent = inputEvent as GamePadPovControllerEvent;
                    if (povEvent?.Index == 0)
                    {
                        povEvent.Button = GamePadButton.Pad;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a mapping from a button index to <see cref="GamePadButton"/>
        /// </summary>
        /// <param name="index">The button index of the button on this device</param>
        /// <param name="button">The button(s) to map to</param>
        protected void AddButtonMapping(int index, GamePadButton button)
        {
            while (buttonMap.Count <= index) buttonMap.Add(GamePadButton.None);
            buttonMap[index] = button;
        }

        /// <summary>
        /// Adds a mapping from an axis index to <see cref="GamePadAxis"/>
        /// </summary>
        /// <param name="index">The axis index of the axis on this device</param>
        /// <param name="axis">The axis/axes to map to</param>
        /// <param name="invert">Should axis be inverted</param>
        protected void AddAxisMapping(int index, GamePadAxis axis, bool invert = false)
        {
            while (axisMap.Count <= index) axisMap.Add(new MappedAxis { Axis = GamePadAxis.None, Invert = false });
            axisMap[index] = new MappedAxis { Axis = axis, Invert = invert };
        }

        private struct MappedAxis
        {
            public GamePadAxis Axis;
            public bool Invert;
        }
    }
}