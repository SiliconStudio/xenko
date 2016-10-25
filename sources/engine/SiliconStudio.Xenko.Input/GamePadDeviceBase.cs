// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Base class for GamePads, contains common functionality for gamepad devices
    /// </summary>
    public abstract class GamePadDeviceBase : IGamePadDevice
    {
        public abstract string DeviceName { get; }
        public abstract Guid Id { get; }
        public int Priority { get; set; }
        public bool Connected => !disposed;
        public int Index => IndexInternal;

        public abstract IReadOnlyCollection<GamePadButtonInfo> ButtonInfos { get; }
        public abstract IReadOnlyCollection<GamePadAxisInfo> AxisInfos { get; }
        public abstract IReadOnlyCollection<GamePadPovControllerInfo> PovControllerInfos { get; }

        public EventHandler OnDisconnect { get; set; }
        public EventHandler<GamePadButtonEvent> OnButton { get; set; }
        public EventHandler<GamePadAxisEvent> OnAxisChanged { get; set; }
        public EventHandler<GamePadPovControllerEvent> OnPovControllerChanged { get; set; }

        internal int IndexInternal;
        private bool disposed;
        protected bool[] buttonStates;
        protected float[] axisStates;
        protected float[] povStates;
        protected bool[] povEnabledStates;

        private readonly List<GamePadInputEvent> gamePadInputEvents = new List<GamePadInputEvent>();
        
        public static float ClampDeadZone(float value, float deadZone)
        {
            if (value > 0.0f)
            {
                value -= deadZone;
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
            }
            else
            {
                value += deadZone;
                if (value > 0.0f)
                {
                    value = 0.0f;
                }
            }

            // Renormalize the value according to the dead zone
            value = value / (1.0f - deadZone);
            return value < -1.0f ? -1.0f : value > 1.0f ? 1.0f : value;
        }

        public void InitializeButtonStates()
        {
            buttonStates = new bool[ButtonInfos.Count];
            axisStates = new float[AxisInfos.Count];
            povStates = new float[PovControllerInfos.Count];
            povEnabledStates = new bool[PovControllerInfos.Count];
        }

        public virtual bool GetButton(int index)
        {
            if (index < 0 || index > buttonStates.Length)
                return false;
            return buttonStates[index];
        }

        public virtual float GetAxis(int index)
        {
            if (index < 0 || index > axisStates.Length)
                return 0.0f;
            return axisStates[index];
        }

        public float GetPovController(int index)
        {
            if (index < 0 || index > povStates.Length)
                return 0.0f;
            return povStates[index];
        }

        public bool GetPovControllerEnabled(int index)
        {
            if (index < 0 || index > povStates.Length)
                return false;
            return povEnabledStates[index];
        }

        public virtual bool GetGamePadState(ref GamePadState state)
        {
            return false;
        }

        /// <summary>
        /// Raise gamepad events collected by Handle____ functions
        /// </summary>
        public virtual void Update()
        {
            // Fire events
            foreach (var evt in gamePadInputEvents)
            {
                if (evt.Type == InputEventType.Button)
                {
                    buttonStates[evt.Index] = evt.State == GamePadButtonState.Pressed;
                    OnButton?.Invoke(this, new GamePadButtonEvent { Index = evt.Index, State = evt.State });
                }
                else if (evt.Type == InputEventType.Axis)
                {
                    axisStates[evt.Index] = evt.Float;
                    OnAxisChanged?.Invoke(this, new GamePadAxisEvent { Index = evt.Index, Value = evt.Float });
                }
                else if (evt.Type == InputEventType.PovController)
                {
                    povStates[evt.Index] = evt.Float;
                    povEnabledStates[evt.Index] = evt.Enabled;
                    OnPovControllerChanged?.Invoke(this, new GamePadPovControllerEvent { Index = evt.Index, Value = evt.Float, Enabled = evt.Enabled});
                }
            }
            gamePadInputEvents.Clear();
        }

        /// <summary>
        /// Marks the device as disconnected
        /// </summary>
        public virtual void Dispose()
        {
            disposed = true;
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
                    State = state ? GamePadButtonState.Pressed : GamePadButtonState.Released
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
            public GamePadButtonState State;
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