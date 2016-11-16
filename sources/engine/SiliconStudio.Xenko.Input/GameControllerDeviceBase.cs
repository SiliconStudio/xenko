// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for gamepads, contains common functionality for gamepad devices
    /// </summary>
    public abstract class GameControllerDeviceBase : IGameControllerDevice
    {
        internal int IndexInternal;
        internal GamePadLayout Layout;
        internal GamePadState State;
        protected bool[] ButtonStates;
        protected float[] AxisStates;
        protected float[] PovStates;
        protected bool[] PovEnabledStates;
        private bool disposed;
        private readonly List<InputEvent> eventQueue = new List<InputEvent>();
        private bool firstStateDetected = false;

        /// <summary>
        /// Marks the device as disconnected and calls <see cref="Disconnected"/>
        /// </summary>
        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Disconnected?.Invoke(this, null);
            }
        }

        public abstract string DeviceName { get; }

        public abstract Guid Id { get; }

        public virtual Guid ProductId => Id;

        public int Priority { get; set; }

        public bool IsConnected => !disposed;

        public int Index => IndexInternal;
        
        GamePadState IGameControllerDevice.State => State;

        public abstract IReadOnlyList<GameControllerButtonInfo> ButtonInfos { get; }

        public abstract IReadOnlyList<GameControllerAxisInfo> AxisInfos { get; }

        public abstract IReadOnlyList<GameControllerPovControllerInfo> PovControllerInfos { get; }
        
        public event EventHandler Disconnected;

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

        /// <summary>
        /// Initializes the gamepad layout
        /// </summary>
        protected void InitializeLayout()
        {
            Layout = GamePadLayouts.FindLayout(this);
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
        
        public virtual bool GetGamePadState(ref GamePadState state)
        {
            if (Layout == null)
                return false;
            state = State;
            return true;
        }

        /// <summary>
        /// Raise gamepad events collected by Handle... functions
        /// </summary>
        public virtual void Update(List<InputEvent> inputEvents)
        {
            // Collect events from queue
            foreach (var evt in eventQueue)
            {
                inputEvents.Add(evt);
            }
            eventQueue.Clear();
            
            if(inputEvents.Count > 0)
                OnAnyObjectChanged();
        }

        protected void MapAndAddEventToQueue<T>(T evt) where T : InputEvent
        {
            InputEvent generatedEvent = null;
            Layout?.MapInputEvent(this, evt, out generatedEvent);
            State.Update(evt);
            eventQueue.Add(evt);
            if (generatedEvent != null)
            {
                eventQueue.Add(generatedEvent);
                State.Update(generatedEvent);
            }
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
                buttonEvent.Button = 0;
                MapAndAddEventToQueue(buttonEvent);
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
                axisEvent.Axis = 0;
                MapAndAddEventToQueue(axisEvent);
            }
        }

        protected void HandlePovController(int index, float state, bool enabled)
        {
            if (index < 0 || index > PovStates.Length)
                throw new IndexOutOfRangeException();
            if (enabled && PovStates[index] != state || PovEnabledStates[index] != enabled)
            {
                PovStates[index] = state;
                var povEvent = InputEventPool<GameControllerPovControllerEvent>.GetOrCreate(this);
                povEvent.Value = state;
                povEvent.Index = index;
                povEvent.Enabled = enabled;
                povEvent.Value = enabled ? state : 0.0f;
                MapAndAddEventToQueue(povEvent);
            }
        }

        private void OnAnyObjectChanged()
        {
            // Kind of a hack to find out when the device actually started reporting data, this event is triggered the first time sends any data that doesn't match the default state
            if (!firstStateDetected)
            {
                for (int i = 0; i < AxisInfos.Count; i++)
                {
                    // Axes that do not idle around -1 are marked bidirectional
                    float floatValue = AxisStates[i];
                    AxisInfos[i].IsBiDirectional = floatValue > -0.75f;
                }

                firstStateDetected = true;
            }
        }
    }
}