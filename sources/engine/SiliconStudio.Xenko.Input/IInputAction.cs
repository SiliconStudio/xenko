// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// An object that can respond to actions from various input gestures (keyboard,mouse,touch,gamepad,etc.)
    /// </summary>
    public abstract class InputAction
    {
        internal InputActionMapping ActionMapping;

        public InputAction()
        {
            Gestures.CollectionChanged += GesturesOnCollectionChanged;
        }

        /// <summary>
        /// The gestures that are used for this action
        /// </summary>
        public TrackingCollection<InputGesture> Gestures { get; } = new TrackingCollection<InputGesture>();

        /// <summary>
        /// The name of the action, as registered in the action mapping
        /// </summary>
        public string MappingName { get; internal set; }

        /// <summary>
        /// Updates the input action, raising events whenever something changed
        /// </summary>
        public abstract void Update();
        
        private void GesturesOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ActionMapping?.AddInputGesture((InputGesture)e.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ActionMapping?.RemoveInputGesture((InputGesture)e.Item);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Gestures collection was modified but the action was not supported by the system.");
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// An action that triggers on a certain condition
    /// </summary>
    public class ButtonAction : InputAction
    {
        /// <summary>
        /// Raised when the action was trigerred
        /// </summary>
        public EventHandler<ButtonState> OnChanged;

        private bool lastValue;

        /// <summary>
        /// The last value of the button action
        /// </summary>
        public bool Value => lastValue;

        public override void Update()
        {
            bool newValue = false;
            foreach (var gesture in Gestures.OfType<IButtonGesture>())
            {
                newValue = newValue || gesture.Button;
            }
            if (lastValue != newValue)
            {
                OnChanged?.Invoke(this, lastValue ? ButtonState.Pressed : ButtonState.Released);
                lastValue = newValue;
            }
        }
    }

    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    public class AxisAction : InputAction
    {
        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        public EventHandler<float> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        public EventHandler<float> OnChanged;

        /// <summary>
        /// The last value of this action
        /// </summary>
        public float Value => lastValue;

        private float lastValue;

        public override void Update()
        {
            float newValue = 0.0f;
            foreach (var gesture in Gestures.OfType<IAxisGesture>())
            {
                if (Math.Abs(gesture.Axis) > Math.Abs(newValue))
                    newValue = gesture.Axis;
            }

            if (lastValue != newValue)
            {
                lastValue = newValue;
                OnChanged?.Invoke(this, lastValue);
            }
            if (lastValue != 0.0f)
            {
                OnNotZero?.Invoke(this, lastValue);
            }
        }
    }

    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    public class DirectionAction : InputAction
    {
        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        public EventHandler<Vector2> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        public EventHandler<Vector2> OnChanged;

        /// <summary>
        /// The last value of this action
        /// </summary>
        public Vector2 Value => lastValue;

        private Vector2 lastValue;

        public override void Update()
        {
            Vector2 dirCombined = Vector2.Zero;
            foreach (var gesture in Gestures.OfType<IDirectionGesture>())
            {
                dirCombined += gesture.Direction;
            }
            float length = dirCombined.Length();
            if (length > 0)
                dirCombined /= length;

            if (lastValue != dirCombined)
            {
                lastValue = dirCombined;
                OnChanged?.Invoke(this, lastValue);
            }
            if (length > 0)
            {
                OnNotZero?.Invoke(this, lastValue);
            }
        }
    }
}