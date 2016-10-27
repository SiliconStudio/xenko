// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An interface for an object that monitors a physical device or other input gesture and generates input for and <see cref="InputAction"/>
    /// </summary>
    [DataContract]
    public abstract class InputGesture
    {
        /// <summary>
        /// Updates the gesture every frame, before receiving inputs through <see cref="IInputEventListener&lt;"/>
        /// </summary>
        /// <param name="elapsedTime">The elapsed time since the last update</param>
        public virtual void Update(TimeSpan elapsedTime)
        {
        }
    }

    /// <summary>
    /// Interface for classes that want to listen to input event of a certain type
    /// </summary>
    /// <typeparam name="TEventType">The type of <see cref="InputEvent"/> that will be sent to this event listener</typeparam>
    public interface IInputEventListener<TEventType> where TEventType : InputEvent
    {
        /// <summary>
        /// Processes a new input event
        /// </summary>
        /// <param name="inputEvent">the input event</param>
        void ProcessEvent(TEventType inputEvent);
    }

    /// <summary>
    /// A gesture that acts as a button, having a true/false state
    /// </summary>
    public interface IButtonGesture
    {
        /// <summary>
        /// The button state of this gesture
        /// </summary>
        bool Button { get; }
    }

    /// <summary>
    /// A gesture that acts as an axis, having a positive or negative float value
    /// </summary>
    public interface IAxisGesture
    {
        /// <summary>
        /// The axis state of this gesture
        /// </summary>
        float Axis { get; }
    }

    /// <summary>
    /// A gesture that acts as a direction, represented as a 2D vector
    /// </summary>
    public interface IDirectionGesture
    {
        /// <summary>
        /// The direction state of this gesture
        /// </summary>
        Vector2 Direction { get; }
    }

    [DataContract]
    public class KeyGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<KeyEvent>
    {
        /// <summary>
        /// Key used for this gesture
        /// </summary>
        public Keys Key;

        private ButtonState currentState = ButtonState.Released;

        public bool Button => currentState == ButtonState.Pressed;
        public float Axis => Button ? 1.0f : 0.0f;
        
        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.Key == Key)
            {
                currentState = inputEvent.State;
            }
        }
    }

    [DataContract]
    public class KeyCombinationGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<KeyEvent>
    {
        [DataMember]
        private HashSet<Keys> keys;
        private readonly HashSet<Keys> heldKeys = new HashSet<Keys>();

        public KeyCombinationGesture()
        {
        }
        public KeyCombinationGesture(params Keys[] keys)
        {
            this.keys = new HashSet<Keys>(keys);
        }

        public bool Button => keys != null && heldKeys.Count == keys.Count;
        public float Axis => Button ? 1.0f : 0.0f;
        
        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (keys?.Contains(inputEvent.Key) ?? false)
            {
                if (inputEvent.State == ButtonState.Pressed)
                    heldKeys.Add(inputEvent.Key);
                else
                    heldKeys.Remove(inputEvent.Key);
            }
        }
    }

    [DataContract]
    public class MouseButtonGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<MouseButtonEvent>
    {
        /// <summary>
        /// Button used for this gesture
        /// </summary>
        public MouseButton MouseButton;

        private ButtonState currentState = ButtonState.Released;

        public bool Button => currentState == ButtonState.Pressed;
        public float Axis => Button ? 1.0f : 0.0f;
        
        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.Button == MouseButton)
            {
                currentState = inputEvent.State;
            }
        }
    }

    [DataContract]
    public class MouseMovementGesture : InputGesture, IAxisGesture, IDirectionGesture, IInputEventListener<PointerEvent>, IInputEventListener<MouseWheelEvent>
    {
        /// <summary>
        /// Axis that is used for this gesture
        /// </summary>
        public MouseAxis MouseAxis;

        private float currentDelta;
        private Vector2 currentDirection;

        public float Axis => currentDelta;
        public Vector2 Direction => currentDirection;

        public override void Update(TimeSpan elapsedTime)
        {
            // Reset delta
            currentDelta = 0;
            currentDirection = Vector2.Zero;
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            switch (MouseAxis)
            {
                case MouseAxis.X:
                    currentDelta = inputEvent.DeltaPosition.X;
                    break;
                case MouseAxis.Y:
                    currentDelta = inputEvent.DeltaPosition.Y;
                    break;
            }
            currentDirection = inputEvent.DeltaPosition;
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            if (MouseAxis == MouseAxis.Wheel)
            {
                currentDelta = inputEvent.WheelDelta;
            }
        }
    }

    [DataContract]
    public class GamePadAxisGesture : InputGesture, IAxisGesture, IInputEventListener<GamePadAxisEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int AxisIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        public int ControllerIndex = 0;

        private float currentState;

        public float Axis => currentState;
        
        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == AxisIndex)
                currentState = inputEvent.Value;
        }
    }

    /// <summary>
    /// A direction or a 0-1 value generated from a gamepad pov controller
    /// </summary>
    [DataContract]
    public class GamePadPovGesture : InputGesture, IAxisGesture, IDirectionGesture, IInputEventListener<GamePadPovControllerEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int AxisIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        public int ControllerIndex = 0;

        private Vector2 currentDirection;
        private float currentState;

        public float Axis => currentState;
        public Vector2 Direction => currentDirection;

        public void ProcessEvent(GamePadPovControllerEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == AxisIndex)
            {
                if (inputEvent.Enabled)
                {
                    currentState = inputEvent.Value;
                    currentDirection = new Vector2((float)Math.Sin(currentState*Math.PI*2), (float)Math.Cos(currentState*Math.PI*2));
                }
                else
                {
                    currentState = 0.0f;
                    currentDirection = Vector2.Zero;
                }
            }
        }
    }
}