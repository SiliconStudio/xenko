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
        protected bool[] buttonStates;
        protected float[] axisStates;
        protected float[] povStates;
        protected bool[] povEnabledStates;
        private bool disposed;
        private readonly List<GamePadInputEvent> gamePadInputEvents = new List<GamePadInputEvent>();

        /// <summary>
        /// Marks the device as disconnected
        /// </summary>
        public virtual void Dispose()
        {
            disposed = true;
        }
        
        /// <inheritdoc />
        public abstract string DeviceName { get; }

        /// <inheritdoc />
        public abstract Guid Id { get; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <inheritdoc />
        public bool Connected => !disposed;

        /// <inheritdoc />
        public int Index => IndexInternal;

        /// <inheritdoc />
        public abstract IReadOnlyList<GamePadButtonInfo> ButtonInfos { get; }

        /// <inheritdoc />
        public abstract IReadOnlyList<GamePadAxisInfo> AxisInfos { get; }

        /// <inheritdoc />
        public abstract IReadOnlyList<GamePadPovControllerInfo> PovControllerInfos { get; }
        
        /// <inheritdoc />
        public EventHandler OnDisconnect { get; set; }

        /// <inheritdoc />
        public EventHandler<GamePadButtonEvent> OnButton { get; set; }

        /// <inheritdoc />
        public EventHandler<GamePadAxisEvent> OnAxisChanged { get; set; }

        /// <inheritdoc />
        public EventHandler<GamePadPovControllerEvent> OnPovControllerChanged { get; set; }

        public void InitializeButtonStates()
        {
            buttonStates = new bool[ButtonInfos.Count];
            axisStates = new float[AxisInfos.Count];
            povStates = new float[PovControllerInfos.Count];
            povEnabledStates = new bool[PovControllerInfos.Count];
        }

        /// <inheritdoc />
        public virtual bool GetButton(int index)
        {
            if (index < 0 || index > buttonStates.Length)
                return false;
            return buttonStates[index];
        }

        /// <inheritdoc />
        public virtual float GetAxis(int index)
        {
            if (index < 0 || index > axisStates.Length)
                return 0.0f;
            return axisStates[index];
        }

        /// <inheritdoc />
        public virtual float GetPovController(int index)
        {
            if (index < 0 || index > povStates.Length)
                return 0.0f;
            return povStates[index];
        }

        /// <inheritdoc />
        public virtual bool GetPovControllerEnabled(int index)
        {
            if (index < 0 || index > povStates.Length)
                return false;
            return povEnabledStates[index];
        }

        /// <inheritdoc />
        public virtual bool GetGamePadState(ref GamePadState state)
        {
            return false;
        }

        /// <summary>
        /// Raise gamepad events collected by Handle... functions
        /// </summary>
        public virtual void Update(List<InputEvent> inputEvents)
        {
            // Fire events
            foreach (var evt in gamePadInputEvents)
            {
                if (evt.Type == InputEventType.Button)
                {
                    buttonStates[evt.Index] = evt.State == ButtonState.Pressed;
                    var buttonEvent = new GamePadButtonEvent(this) { State = evt.State, Index = evt.Index};
                    OnButton?.Invoke(this, buttonEvent);
                    inputEvents.Add(buttonEvent);
                }
                else if (evt.Type == InputEventType.Axis)
                {
                    axisStates[evt.Index] = evt.Float;
                    var axisEvent = new GamePadAxisEvent(this) { Index = evt.Index, Value = evt.Float};
                    OnAxisChanged?.Invoke(this, axisEvent);
                    inputEvents.Add(axisEvent);
                }
                else if (evt.Type == InputEventType.PovController)
                {
                    povStates[evt.Index] = evt.Float;
                    povEnabledStates[evt.Index] = evt.Enabled;
                    var povEvent = new GamePadPovControllerEvent (this) { Index = evt.Index, Value = evt.Float, Enabled = evt.Enabled };
                    OnPovControllerChanged?.Invoke(this, povEvent);
                    inputEvents.Add(povEvent);
                }
            }
            gamePadInputEvents.Clear();
        }
        
        protected void HandleButton(int index, bool state)
        {
            if (index < 0 || index > buttonStates.Length)
                throw new IndexOutOfRangeException();
            if (buttonStates[index] != state)
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.Button,
                    State = state ? ButtonState.Pressed : ButtonState.Released
                });
        }

        protected void HandleAxis(int index, float state)
        {
            if (index < 0 || index > axisStates.Length)
                throw new IndexOutOfRangeException();
            if (axisStates[index] != state)
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.Axis,
                    Float = state
                });
        }

        protected void HandlePovController(int index, float state, bool enabled)
        {
            if (index < 0 || index > povStates.Length)
                throw new IndexOutOfRangeException();
            if (povStates[index] != state || povEnabledStates[index] != enabled)
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