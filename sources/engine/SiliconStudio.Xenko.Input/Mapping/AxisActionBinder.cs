// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Generates gestures mapping to axis actions, bindings are made in the order Up/Down (or first stick movement binds to the positive direction)
    /// </summary>
    public class AxisActionBinder : ActionBinder,
        IInputEventListener<GamePadButtonEvent>,
        IInputEventListener<KeyEvent>,
        IInputEventListener<MouseButtonEvent>,
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
        /// Creates a new axis action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="usedGestures">A set of already used gesture that are filtered out from the input, can be null</param>
        public AxisActionBinder(InputManager inputManager, HashSet<IInputGesture> usedGestures = null) : base(inputManager, usedGestures)
        {
            inputManager.AddListener(this);
        }
        
        public override int NumBindings { get; } = 2;
        public override bool AcceptsAxes => TargetGesture == null;
        public override bool AcceptsButtons => TargetGesture == null || TargetGesture is TwoWayGesture;
        public override bool AcceptsDirections => false;

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
                TryBindSingleButton(new GamePadButtonGesture(inputEvent.Index) {ControllerIndex = inputEvent.GamePad.Index});
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
                TryBindSingleButton(new KeyGesture(inputEvent.Key));
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
                TryBindSingleButton(new MouseButtonGesture(inputEvent.Button));
        }

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (Math.Abs(inputEvent.Value) > AxisThreshold)
                TryBindAxis(new GamePadAxisGesture(inputEvent.Index) { Inverted = inputEvent.Value < 0, ControllerIndex = inputEvent.GamePad.Index });
        }

        public void ProcessEvent(PointerEvent inputEvent)
        {
            // Only accept mouse movement as axis, touch should use gestures instead
            if (inputEvent.PointerType != PointerType.Mouse) return;

            Vector2 absDelta = inputEvent.DeltaPosition*inputEvent.Pointer.SurfaceSize;
            if (Math.Abs(absDelta.X) > PointerThreshold)
            {
                TryBindAxis(new MouseAxisGesture(MouseAxis.X) { Inverted = absDelta.X < 0 });
            }
            else if (Math.Abs(absDelta.Y) > PointerThreshold)
            {
                TryBindAxis(new MouseAxisGesture(MouseAxis.Y) { Inverted = absDelta.Y < 0 });
            }
        }

        protected virtual void TryBindAxis(IAxisGesture axis)
        {
            // Filter out duplicate axes
            if (UsedGestures.Contains(axis)) return;

            if (TargetGesture == null)
            {
                TargetGesture = axis;
                UsedGestures.Add(axis);
                Advance(2);
            }
        }

        protected virtual TwoWayGesture AsTwoWayGesture()
        {
            if (TargetGesture == null)
                TargetGesture = new TwoWayGesture();
            return TargetGesture as TwoWayGesture;
        }

        protected virtual void TryBindSingleButton(IButtonGesture button)
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
            }

            UsedGestures.Add(button);
            Advance();
        }
    }
}