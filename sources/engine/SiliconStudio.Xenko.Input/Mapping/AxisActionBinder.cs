// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Generates gestures mapping to axis actions, bindings are made in the order Up/Down (or first stick movement binds to the positive direction)
    /// </summary>
    public class AxisActionBinder : ActionBinder,
        IInputEventListener<KeyEvent>,
        IInputEventListener<MouseButtonEvent>,
        IInputEventListener<GameControllerButtonEvent>,
        IInputEventListener<GamePadButtonEvent>,
        IInputEventListener<GameControllerAxisEvent>,
        IInputEventListener<GamePadAxisEvent>,
        IInputEventListener<PointerEvent>
    {
        /// <summary>
        /// The threshold (in absolute device coordinates) that is used to detect pointer movement
        /// </summary>
        public float PointerThreshold = 4.0f;

        /// <summary>
        /// The threshold that is used to trigger axes
        /// </summary>
        public float AxisThreshold = 0.9f;

        /// <summary>
        /// If true, when a single direction axis is used, both a positive and negative binding will be used
        /// </summary>
        public bool RequireBidirectionAxis = true;

        /// <summary>
        /// Creates a new axis action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="usedGestures">A set of already used gesture that are filtered out from the input, can be null</param>
        public AxisActionBinder(InputManager inputManager, HashSet<InputGesture> usedGestures = null) : base(inputManager, usedGestures)
        {
            inputManager.AddListener(this);
        }
        
        public override int BindingCount { get; } = 2;
        public override bool AcceptsAxes => TargetGesture == null;
        public override bool AcceptsButtons => TargetGesture == null || TargetGesture is TwoWayGesture;
        public override bool AcceptsDirections => false;


        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.IsDown)
                TryBindSingleButton(new KeyGesture(inputEvent.Key));
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
                TryBindSingleButton(new MouseButtonGesture(inputEvent.Button));
        }

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
                TryBindSingleButton(new GameControllerButtonGesture(inputEvent.Index, inputEvent.GameController.Id));
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
                TryBindSingleButton(new GamePadButtonGesture(inputEvent.Button, inputEvent.GamePad.Index));
        }

        public void ProcessEvent(GameControllerAxisEvent inputEvent)
        {
            if (Math.Abs(inputEvent.Value) > AxisThreshold)
            {
                var axis = new GameControllerAxisGesture(inputEvent.Index, inputEvent.GameController.Id) { Inverted = inputEvent.Value < 0 };
                TryBindAxis(axis);
            }
        }

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (Math.Abs(inputEvent.Value) > AxisThreshold)
            {
                var axis = new GamePadAxisGesture(inputEvent.Axis, inputEvent.GamePad.Index) { Inverted = inputEvent.Value < 0 };
                TryBindAxis(axis, inputEvent.Axis < GamePadAxis.LeftTrigger);
            }
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            // Only accept mouse movement as axis, touch should use gestures instead
            if (inputEvent.PointerType != PointerType.Mouse) return;

            Vector2 absDelta = inputEvent.AbsoluteDeltaPosition;
            if (Math.Abs(absDelta.X) > PointerThreshold)
            {
                TryBindAxis(new MouseAxisGesture(MouseAxis.X) { Inverted = absDelta.X < 0 });
            }
            else if (Math.Abs(absDelta.Y) > PointerThreshold)
            {
                TryBindAxis(new MouseAxisGesture(MouseAxis.Y) { Inverted = absDelta.Y < 0 });
            }
        }

        protected virtual void TryBindAxis(AxisGesture axis, bool isBidirectional = true)
        {
            // Filter out duplicate axes
            if (UsedGestures.Contains(axis)) return;

            if (TargetGesture == null)
            {
                // Handle single directional axes, such as gamepad triggers
                // this allows users to bind two triggers to a single (positive/negative) axis
                if (RequireBidirectionAxis && !isBidirectional)
                {
                    if (TargetGesture != null) return;
                    // Create compound gesture
                    var compound = new CompoundAxisGesture();
                    compound.Gestures.Add(axis);
                    TargetGesture = compound;
                    Advance(1);
                }
                else
                {
                    TargetGesture = axis;
                    Advance(2);
                }
            }
            else if (TargetGesture is CompoundAxisGesture)
            {
                ((CompoundAxisGesture)TargetGesture).Gestures.Add(axis);
                // Invert axis since this is now being used as the negative trigger
                var scalable = axis as AxisGesture;
                if (scalable != null) scalable.Inverted = !scalable.Inverted;
                Advance(1);
            }

            UsedGestures.Add(axis);
        }

        protected virtual TwoWayGesture AsTwoWayGesture()
        {
            if (TargetGesture == null)
            {
                TargetGesture = new TwoWayGesture();
            }

            return TargetGesture as TwoWayGesture;
        }

        protected virtual void TryBindSingleButton(ButtonGesture button)
        {
            // Filter out duplicate buttons
            if (UsedGestures.Contains(button)) return;

            var gesture = AsTwoWayGesture();
            if (gesture != null)
            {
                if (gesture.Positive == null)
                {
                    gesture.Positive = button;
                }
                else
                {
                    gesture.Negative = button;
                }

                UsedGestures.Add(button);
                Advance();
            }
        }
    }
}