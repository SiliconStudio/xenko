// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
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

        protected Direction[] DirectionStates;

        public abstract string Name { get; }

        public abstract Guid Id { get; }

        public virtual Guid ProductId => Id;

        public int Priority { get; set; }

        public abstract IInputSource Source { get; }

        public abstract IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

        public abstract IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

        public abstract IReadOnlyList<GameControllerDirectionInfo> DirectionInfos { get; }

        /// <summary>
        /// Creates the correct amount of states based on the amount of object infos that are set
        /// </summary>
        protected void InitializeButtonStates()
        {
            ButtonStates = new bool[ButtonInfos.Count];
            AxisStates = new float[AxisInfos.Count];
            DirectionStates = new Direction[DirectionInfos.Count];
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
        
        public virtual Direction GetDirection(int index)
        {
            return DirectionStates[index];
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
                buttonEvent.IsDown = state;
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

        protected void HandleDirection(int index, Direction state)
        {
            if (index < 0 || index > DirectionStates.Length)
                throw new IndexOutOfRangeException();

            if (DirectionStates[index] != state)
            {
                DirectionStates[index] = state;
                var directionEvent = InputEventPool<GameControllerDirectionEvent>.GetOrCreate(this);
                directionEvent.Index = index;
                directionEvent.Direction = state;
                events.Add(directionEvent);
            }
        }
    }
}