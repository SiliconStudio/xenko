// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for gamepads, contains common functionality for gamepad devices
    /// </summary>
    public abstract class GamePadDeviceBase : IGamePadDevice
    {
        internal int IndexInternal;
        internal GamePadLayout Layout;
        internal GamePadState State;
        protected bool[] ButtonStates;
        protected float[] AxisStates;
        protected float[] PovStates;
        protected bool[] PovEnabledStates;
        private bool disposed;
        private readonly List<GamePadInputEvent> gamePadInputEvents = new List<GamePadInputEvent>();
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

        public bool Connected => !disposed;

        public int Index => IndexInternal;

        public abstract IReadOnlyList<GamePadButtonInfo> ButtonInfos { get; }

        public abstract IReadOnlyList<GamePadAxisInfo> AxisInfos { get; }

        public abstract IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos { get; }
        
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
            // Fire events
            foreach (var evt in gamePadInputEvents)
            {
                InputEvent generatedEvent = null;
                if (evt.Type == InputEventType.Button)
                {
                    ButtonStates[evt.Index] = evt.State == ButtonState.Pressed;
                    var buttonEvent = InputEventPool<GamePadButtonEvent>.GetOrCreate(this);
                    buttonEvent.State = evt.State;
                    buttonEvent.Index = evt.Index;
                    buttonEvent.Button = 0;
                    Layout?.MapInputEvent(this, buttonEvent, out generatedEvent);
                    State.Update(buttonEvent);
                    inputEvents.Add(buttonEvent);
                }
                else if (evt.Type == InputEventType.Axis)
                {
                    AxisStates[evt.Index] = evt.Float;
                    var axisEvent = InputEventPool<GamePadAxisEvent>.GetOrCreate(this);
                    axisEvent.Index = evt.Index;
                    axisEvent.Value = evt.Float;
                    axisEvent.Axis = 0;
                    Layout?.MapInputEvent(this, axisEvent, out generatedEvent);
                    State.Update(axisEvent);
                    inputEvents.Add(axisEvent);
                }
                else if (evt.Type == InputEventType.PovController)
                {
                    PovStates[evt.Index] = evt.Float;
                    PovEnabledStates[evt.Index] = evt.Enabled;
                    var povEvent = InputEventPool<GamePadPovControllerEvent>.GetOrCreate(this);
                    povEvent.Index = evt.Index;
                    povEvent.Value = evt.Float;
                    povEvent.Button = 0;
                    povEvent.Enabled = evt.Enabled;
                    Layout?.MapInputEvent(this, povEvent, out generatedEvent);
                    State.Update(povEvent);
                    inputEvents.Add(povEvent);
                }

                if (generatedEvent != null)
                {
                    State.Update(generatedEvent);
                    inputEvents.Add(generatedEvent);
                }
            }
            if(gamePadInputEvents.Count > 0)
                OnAnyObjectChanged();
            gamePadInputEvents.Clear();
        }

        protected void HandleButton(int index, bool state)
        {
            if (index < 0 || index > ButtonStates.Length)
                throw new IndexOutOfRangeException();
            if (ButtonStates[index] != state)
            {
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.Button,
                    State = state ? ButtonState.Pressed : ButtonState.Released
                });
            }
        }

        protected void HandleAxis(int index, float state)
        {
            if (index < 0 || index > AxisStates.Length)
                throw new IndexOutOfRangeException();
            if (AxisStates[index] != state)
            {
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.Axis,
                    Float = state
                });
            }
        }

        protected void HandlePovController(int index, float state, bool enabled)
        {
            if (index < 0 || index > PovStates.Length)
                throw new IndexOutOfRangeException();
            if (enabled && PovStates[index] != state || PovEnabledStates[index] != enabled)
            {
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.PovController,
                    Float = enabled ? state : 0.0f,
                    Enabled = enabled
                });
            }
        }

        private void OnAnyObjectChanged()
        {
            // Kind of a hack to find out when the device actually started reporting data, this event is triggered the first time sends any data that doesn't match the default state
            if (!firstStateDetected)
            {
                for (int i = 0; i < AxisInfos.Count; i++)
                {
                    // TODO: Test this
                    // Axes that do not idle around -1 are marked bidirectional
                    float floatValue = AxisStates[i];
                    AxisInfos[i].IsBiDirectional = floatValue > -0.75f;
                }

                firstStateDetected = true;
            }
        }

        protected struct GamePadInputEvent
        {
            public InputEventType Type;
            public float Float;
            public int Int;
            public bool Enabled;
            public ButtonState State;
            public int Index;
        }

        protected enum InputEventType
        {
            Button,
            Axis,
            PovController
        }
    }
}