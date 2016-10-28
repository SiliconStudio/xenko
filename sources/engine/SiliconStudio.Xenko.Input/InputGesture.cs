// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An interface for an object that monitors a physical device or other input gesture and generates input for and <see cref="InputAction"/>
    /// </summary>
    public interface IInputGesture
    {
        /// <summary>
        /// Allows the gesture to reset states, e.g. putting delta input values back on zero
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Base class of <see cref="IInputGesture"/>, every input gesture should inherit from this class if you want to use them with <see cref="InputActionMapping"/>
    /// </summary>
    [DataContract]
    public abstract class InputGesture : IInputGesture
    {
        internal InputActionMapping ActionMapping;
        private List<InputGesture> childGestures = new List<InputGesture>();

        /// <inheritdoc />
        public virtual void Reset()
        {
        }

        internal void OnAdded()
        {
            ActionMapping.AddInputGesture(this);
            foreach (var child in childGestures)
            {
                child.ActionMapping = ActionMapping;
                child.OnAdded();
            }
        }

        internal void OnRemoved()
        {
            ActionMapping.RemoveInputGesture(this);
            ActionMapping = null;
            foreach (var child in childGestures)
            {
                child.OnRemoved();
            }
        }

        protected void AddChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGesture;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGesture");
            childGestures.Add(gesture);
            if (ActionMapping != null)
                gesture.OnAdded();
        }

        protected void RemoveChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGesture;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGesture");
            if (ActionMapping != null)
                gesture.OnRemoved();
            childGestures.Remove(gesture);
        }

        protected void UpdateChild(IInputGesture oldChild, IInputGesture newChild)
        {
            if (oldChild != null)
                RemoveChild(oldChild);
            if (newChild != null)
                AddChild(newChild);
        }
    }

    /// <summary>
    /// A gesture that acts as a button, having a true/false state
    /// </summary>
    public interface IButtonGesture : IInputGesture
    {
        /// <summary>
        /// The button state of this gesture
        /// </summary>
        bool Button { get; }
    }

    /// <summary>
    /// A gesture that acts as an axis, having a positive or negative float value
    /// </summary>
    public interface IAxisGesture : IInputGesture
    {
        /// <summary>
        /// The axis state of this gesture
        /// </summary>
        float Axis { get; }
    }

    /// <summary>
    /// A gesture that acts as a direction, represented as a 2D vector
    /// </summary>
    public interface IDirectionGesture : IInputGesture
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

        public KeyGesture()
        {
        }

        public KeyGesture(Keys key)
        {
            this.Key = key;
        }

        public bool Button => currentState == ButtonState.Pressed;
        public float Axis => Button ? 1.0f : 0.0f;

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.Key == Key)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(KeyGesture other)
        {
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)Key;
        }
    }

    [DataContract]
    public class TwoWayGesture : InputGesture, IAxisGesture
    {
        private IButtonGesture positive;
        private IButtonGesture negative;

        public IButtonGesture Positive
        {
            get { return positive; }
            set
            {
                UpdateChild(positive, value);
                positive = value;
            }
        }

        public IButtonGesture Negative
        {
            get { return negative; }
            set
            {
                UpdateChild(negative, value);
                negative = value;
            }
        }

        public float Axis => Positive?.Button ?? false ? 1.0f : (Negative?.Button ?? false ? -1.0f : 0.0f);

        public override void Reset()
        {
            positive?.Reset();
            negative?.Reset();
        }

        public override string ToString()
        {
            return $"{nameof(Positive)}: ({Positive}), {nameof(Negative)}: ({Negative}), {nameof(Axis)}: {Axis}";
        }
    }

    [DataContract]
    public class FourWayGesture : InputGesture, IDirectionGesture
    {
        private IAxisGesture y;
        private IAxisGesture x;

        public IAxisGesture X
        {
            get { return x; }
            set
            {
                UpdateChild(x, value);
                x = value;
            }
        }

        public IAxisGesture Y
        {
            get { return y; }
            set
            {
                UpdateChild(y, value);
                y = value;
            }
        }

        public bool Normalized { get; set; } = true;

        public Vector2 Direction
        {
            get
            {
                var vec = new Vector2(X?.Axis ?? 0.0f, Y?.Axis ?? 0.0f);
                if (Normalized)
                {
                    float length = vec.Length();
                    if (length > 1.0f)
                        vec /= length;
                }
                return vec;
            }
        }

        public override void Reset()
        {
            x?.Reset();
            y?.Reset();
        }

        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Normalized)}: {Normalized}, {nameof(Direction)}: {Direction}";
        }
    }

    [DataContract]
    public class KeyCombinationGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<KeyEvent>
    {
        [DataMember] private HashSet<Keys> keys;
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

        public override string ToString()
        {
            return $"Keys: {string.Join(", ", keys)}, Held Keys: {string.Join(", ", heldKeys)}";
        }

        protected bool Equals(KeyCombinationGesture other)
        {
            return Equals(keys, other.keys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeyCombinationGesture)obj);
        }

        public override int GetHashCode()
        {
            return (keys != null ? keys.GetHashCode() : 0);
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

        public MouseButtonGesture()
        {
        }

        public MouseButtonGesture(MouseButton button)
        {
            MouseButton = button;
        }

        public bool Button => currentState == ButtonState.Pressed;
        public float Axis => Button ? 1.0f : 0.0f;

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.Button == MouseButton)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(MouseButton)}: {MouseButton}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(MouseButtonGesture other)
        {
            return MouseButton == other.MouseButton;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)MouseButton;
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

        public MouseMovementGesture()
        {
        }

        public MouseMovementGesture(MouseAxis axis)
        {
            MouseAxis = axis;
        }

        public float Axis => Inverted ? -currentDelta : currentDelta;
        public Vector2 Direction => currentDirection;
        public bool Inverted { get; set; } = false;

        public override void Reset()
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

        public override string ToString()
        {
            return $"{nameof(MouseAxis)}: {MouseAxis}, {nameof(Axis)}: {Axis}, {nameof(Direction)}: {Direction}, {nameof(Inverted)}: {Inverted}";
        }

        protected bool Equals(MouseMovementGesture other)
        {
            return MouseAxis == other.MouseAxis;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MouseMovementGesture)obj);
        }

        public override int GetHashCode()
        {
            return (int)MouseAxis;
        }
    }

    [DataContract]
    public class AxisButtonGesture : InputGesture, IButtonGesture
    {
        public float Threshold = 0.5f;
        private IAxisGesture axis;

        public IAxisGesture Axis
        {
            get { return axis; }
            set
            {
                UpdateChild(axis, value);
                axis = value;
            }
        }

        public bool Button => Axis?.Axis > Threshold;

        public override void Reset()
        {
            axis?.Reset();
        }

        protected bool Equals(AxisButtonGesture other)
        {
            return Threshold.Equals(other.Threshold);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AxisButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            return Threshold.GetHashCode();
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

        public GamePadAxisGesture()
        {
        }

        public GamePadAxisGesture(int axisIndex)
        {
            AxisIndex = axisIndex;
        }

        public float Axis => Inverted ? -currentState : currentState;
        public bool Inverted { get; set; } = false;

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == AxisIndex)
                currentState = inputEvent.Value;
        }

        public override string ToString()
        {
            return $"{nameof(AxisIndex)}: {AxisIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}";
        }

        protected bool Equals(GamePadAxisGesture other)
        {
            return AxisIndex == other.AxisIndex && ControllerIndex == other.ControllerIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadAxisGesture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (AxisIndex*397) ^ ControllerIndex;
            }
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
        public int PovIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        public int ControllerIndex = 0;

        private Vector2 currentDirection;
        private float currentState;

        public GamePadPovGesture()
        {
        }

        public GamePadPovGesture(int povIndex)
        {
            PovIndex = povIndex;
        }

        public float Axis => currentState;
        public Vector2 Direction => currentDirection;

        public void ProcessEvent(GamePadPovControllerEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == PovIndex)
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

        public override string ToString()
        {
            return $"{nameof(PovIndex)}: {PovIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Axis)}: {Axis}, {nameof(Direction)}: {Direction}";
        }

        protected bool Equals(GamePadPovGesture other)
        {
            return PovIndex == other.PovIndex && ControllerIndex == other.ControllerIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadPovGesture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (PovIndex*397) ^ ControllerIndex;
            }
        }
    }


    /// <summary>
    /// A button gesture generated from a gamepad button press
    /// </summary>
    [DataContract]
    public class GamePadButtonGesture : InputGesture, IButtonGesture, IAxisGesture, IInputEventListener<GamePadButtonEvent>
    {
        /// <summary>
        /// The index of the axis to use
        /// </summary>
        public int ButtonIndex = 0;

        /// <summary>
        /// The controller index
        /// </summary>
        public int ControllerIndex = 0;

        private ButtonState currentState;

        public GamePadButtonGesture()
        {
        }

        public GamePadButtonGesture(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }

        public float Axis => Button ? 1.0f : 0.0f;
        public bool Button => currentState == ButtonState.Pressed;

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.GamePad.Index == ControllerIndex && inputEvent.Index == ButtonIndex)
            {
                currentState = inputEvent.State;
            }
        }

        public override string ToString()
        {
            return $"{nameof(ButtonIndex)}: {ButtonIndex}, {nameof(ControllerIndex)}: {ControllerIndex}, {nameof(Button)}: {Button}";
        }

        protected bool Equals(GamePadButtonGesture other)
        {
            return ButtonIndex == other.ButtonIndex && ControllerIndex == other.ControllerIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GamePadButtonGesture)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ButtonIndex*397) ^ ControllerIndex;
            }
        }
    }
}