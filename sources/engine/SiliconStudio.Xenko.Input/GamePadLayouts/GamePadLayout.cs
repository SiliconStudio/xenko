// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

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
        /// Compares a product id
        /// </summary>
        /// <param name="a">id a</param>
        /// <param name="b">id b</param>
        /// <param name="numBytes">number of bytes to compare, starting from <paramref name="byteOffset"/></param>
        /// <param name="byteOffset">starting byte index from where to compare</param>
        /// <returns></returns>
        public static bool CompareProductId(Guid a, Guid b, int numBytes = 16, int byteOffset = 0)
        {
            byte[] aBytes = a.ToByteArray();
            byte[] bBytes = b.ToByteArray();
            byteOffset = MathUtil.Clamp(byteOffset, 0, aBytes.Length);
            numBytes = MathUtil.Clamp(byteOffset+numBytes, 0, aBytes.Length) - byteOffset;
            for (int i = byteOffset; i < numBytes; i++)
                if (aBytes[i] != bBytes[i]) return false;
            return true;
        }

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
        /// <param name="generatedEvent">Optionally a generated event to respond to this input event</param>
        public virtual void MapInputEvent(IGamePadDevice device, InputEvent inputEvent, out InputEvent generatedEvent)
        {
            generatedEvent = null;
            var buttonEvent = inputEvent as GamePadButtonEvent;
            if (buttonEvent != null)
            {
                if (buttonEvent.Index < buttonMap.Count)
                {
                    buttonEvent.Button = buttonMap[buttonEvent.Index];
                }
            }
            else
            {
                var axisEvent = inputEvent as GamePadAxisEvent;
                if (axisEvent != null)
                {
                    if (axisEvent.Index < axisMap.Count)
                    {
                        var mappedAxis = axisMap[axisEvent.Index];
                        if (mappedAxis.Invert)
                        {
                            // Create new event that has the axis inverted
                            var axisEvent1 = InputEventPool<GamePadAxisEvent>.GetOrCreate(device);
                            generatedEvent = axisEvent1;
                            axisEvent1.Axis = mappedAxis.Axis;
                            axisEvent1.Index = -1;
                            axisEvent1.Value = -axisEvent.Value;
                        }
                        else
                        {
                            axisEvent.Axis = mappedAxis.Axis;
                        }
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