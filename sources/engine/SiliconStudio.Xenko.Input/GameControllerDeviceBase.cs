// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for gamepads, contains common functionality for gamepad devices
    /// </summary>
    public abstract class GameControllerDeviceBase : IGameControllerDevice
    {
        private readonly List<InputEvent> events = new List<InputEvent>();

        protected bool[] ButtonStates;

        protected float[] AxisStates;

        protected float[] PovStates;

        protected bool[] PovEnabledStates;

        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public virtual Guid ProductId => Id;

        public int Priority { get; set; }

        public abstract IInputSource Source { get; }

        public abstract IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

        public abstract IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

        public abstract IReadOnlyList<PovControllerInfo> PovControllerInfos { get; }

        /// <summary>
        /// Creates the correct amount of states based on the amount of object infos that are set
        /// </summary>
        protected void InitializeButtonStates()
        {
            ButtonStates = new bool[ButtonInfos.Count];
            AxisStates = new float[AxisInfos.Count];
            PovStates = new float[PovControllerInfos.Count];
            PovEnabledStates = new bool[PovControllerInfos.Count];
        }
        
        public virtual bool GetButton(int index)
        {
            if (index < 0 || index > ButtonStates.Length)
                return false;

            return ButtonStates[index];
        }
        
        public virtual float GetAxis(int index)
        {
            if (index < 0 || index > AxisStates.Length)
                return 0.0f;

            return AxisStates[index];
        }
        
        public virtual float GetPovController(int index)
        {
            if (index < 0 || index > PovStates.Length)
                return 0.0f;

            return PovStates[index];
        }
        
        public virtual bool GetPovControllerEnabled(int index)
        {
            if (index < 0 || index > PovStates.Length)
                return false;

            return PovEnabledStates[index];
        }

        /// <summary>
        /// Raise gamepad events collected by Handle... functions
        /// </summary>
        public virtual void Update(List<InputEvent> inputEvents)
        {
            // Collect events from queue
            foreach (var evt in events)
            {
                inputEvents.Add(evt);
            }
            events.Clear();
        }

        protected void HandleButton(int index, bool state)
        {
            if (index < 0 || index > ButtonStates.Length)
                throw new IndexOutOfRangeException();

            if (ButtonStates[index] != state)
            {
                ButtonStates[index] = state;
                var buttonEvent = InputEventPool<GameControllerButtonEvent>.GetOrCreate(this);
                buttonEvent.State = state ? ButtonState.Down : ButtonState.Up;
                buttonEvent.Index = index;
                events.Add(buttonEvent);
            }
        }

        protected void HandleAxis(int index, float state)
        {
            if (index < 0 || index > AxisStates.Length)
                throw new IndexOutOfRangeException();

            if (AxisStates[index] != state)
            {
                AxisStates[index] = state;
                var axisEvent = InputEventPool<GameControllerAxisEvent>.GetOrCreate(this);
                axisEvent.Value = state;
                axisEvent.Index = index;
                events.Add(axisEvent);
            }
        }

        protected void HandlePovController(int index, float state, bool enabled)
        {
            if (index < 0 || index > PovStates.Length)
                throw new IndexOutOfRangeException();

            if (enabled && PovStates[index] != state || PovEnabledStates[index] != enabled)
            {
                PovStates[index] = state;
                PovEnabledStates[index] = enabled;
                var povEvent = InputEventPool<PovControllerEvent>.GetOrCreate(this);
                povEvent.Value = state;
                povEvent.Index = index;
                povEvent.Enabled = enabled;
                povEvent.Value = enabled ? state : 0.0f;
                events.Add(povEvent);
            }
        }
    }
}