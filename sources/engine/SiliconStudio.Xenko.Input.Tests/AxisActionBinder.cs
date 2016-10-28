// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Generates gestures mapping to axis actions
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

        protected string[] names;
        protected HashSet<IInputGesture> usedGestures = new HashSet<IInputGesture>();

        /// <summary>
        /// Creates a new axis action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        public AxisActionBinder(InputManager inputManager) : base(inputManager)
        {
            names = new[] { "Up", "Down" };
            inputManager.AddListener(this);
        }

        /// <summary>
        /// Creates a new direction action binder with custom direction names
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="pos">Label for positive direction</param>
        /// <param name="neg">Label for negative direction</param>
        public AxisActionBinder(InputManager inputManager, string pos, string neg) : base(inputManager)
        {
            names = new[] { pos, neg };
            inputManager.AddListener(this);
        }

        public override string NextName => names[MathUtil.Clamp(Index, 0, 1)];
        public override int NumBindings { get; } = 2;
        public override bool AcceptsAxes => targetGesture == null;
        public override bool AcceptsButtons => targetGesture == null || targetGesture is TwoWayGesture;
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
                TryBindAxis(new MouseMovementGesture(MouseAxis.X) { Inverted = absDelta.X < 0 });
            }
            else if (Math.Abs(absDelta.Y) > PointerThreshold)
            {
                TryBindAxis(new MouseMovementGesture(MouseAxis.Y) { Inverted = absDelta.Y < 0 });
            }
        }

        protected virtual void TryBindAxis(IAxisGesture axis)
        {
            // Filter out duplicate axes
            if (usedGestures.Contains(axis)) return;

            if (targetGesture == null)
            {
                targetGesture = axis;
                usedGestures.Add(axis);
                Advance(2);
            }
        }

        protected virtual TwoWayGesture AsTwoWayGesture()
        {
            if (targetGesture == null)
                targetGesture = new TwoWayGesture();
            return targetGesture as TwoWayGesture;
        }

        protected virtual void TryBindSingleButton(IButtonGesture button)
        {
            // Filter out duplicate buttons
            if (usedGestures.Contains(button)) return;

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

            usedGestures.Add(button);
            Advance();
        }
    }
}