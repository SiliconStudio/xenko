// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for gamepads, contains common functionality for gamepad devices
    /// </summary>
    public abstract class GamePadDeviceBase : IGamePadDevice
    {
        internal int IndexInternal;
        protected bool[] ButtonStates;
        protected float[] AxisStates;
        protected float[] PovStates;
        protected bool[] PovEnabledStates;
        private bool disposed;
        private readonly List<GamePadInputEvent> gamePadInputEvents = new List<GamePadInputEvent>();
        private GamePadLayout layout;

        /// <summary>
        /// Marks the device as disconnected
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
            layout = GamePadLayoutRegistry.FindLayout(this);
        }

        /// <inheritdoc />
        public virtual bool GetButton(int index)
        {
            if (index < 0 || index > ButtonStates.Length)
                return false;
            return ButtonStates[index];
        }

        /// <inheritdoc />
        public virtual float GetAxis(int index)
        {
            if (index < 0 || index > AxisStates.Length)
                return 0.0f;
            return AxisStates[index];
        }

        /// <inheritdoc />
        public virtual float GetPovController(int index)
        {
            if (index < 0 || index > PovStates.Length)
                return 0.0f;
            return PovStates[index];
        }

        /// <inheritdoc />
        public virtual bool GetPovControllerEnabled(int index)
        {
            if (index < 0 || index > PovStates.Length)
                return false;
            return PovEnabledStates[index];
        }

        /// <inheritdoc />
        public virtual bool GetGamePadState(ref GamePadState state)
        {
            if (layout == null)
                return false;
            layout.GetState(this, ref state);
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
                if (evt.Type == InputEventType.Button)
                {
                    ButtonStates[evt.Index] = evt.State == ButtonState.Pressed;
                    var buttonEvent = new GamePadButtonEvent(this) { State = evt.State, Index = evt.Index };
                    inputEvents.Add(buttonEvent);
                }
                else if (evt.Type == InputEventType.Axis)
                {
                    AxisStates[evt.Index] = evt.Float;
                    var axisEvent = new GamePadAxisEvent(this) { Index = evt.Index, Value = evt.Float };
                    inputEvents.Add(axisEvent);
                }
                else if (evt.Type == InputEventType.PovController)
                {
                    PovStates[evt.Index] = evt.Float;
                    PovEnabledStates[evt.Index] = evt.Enabled;
                    var povEvent = new GamePadPovControllerEvent(this) { Index = evt.Index, Value = evt.Float, Enabled = evt.Enabled };
                    inputEvents.Add(povEvent);
                }
            }
            gamePadInputEvents.Clear();
        }

        protected void HandleButton(int index, bool state)
        {
            if (index < 0 || index > ButtonStates.Length)
                throw new IndexOutOfRangeException();
            if (ButtonStates[index] != state)
                gamePadInputEvents.Add(new GamePadInputEvent
                {
                    Index = index,
                    Type = InputEventType.Button,
                    State = state ? ButtonState.Pressed : ButtonState.Released
                });
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