// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Generates gestures mapping to button actions
    /// </summary>
    public class ButtonActionBinder : ActionBinder,
        IInputEventListener<GamePadButtonEvent>,
        IInputEventListener<KeyEvent>,
        IInputEventListener<MouseButtonEvent>,
        IInputEventListener<GamePadAxisEvent>,
        IInputEventListener<MouseWheelEvent>
    {
        /// <summary>
        /// The threshold that is used to trigger using axes as buttons
        /// </summary>
        public float AxisThreshold = 0.9f;

        /// <summary>
        /// Creates a new button action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        public ButtonActionBinder(InputManager inputManager) : base(inputManager)
        {
            inputManager.AddListener(this);
        }

        /// <summary>
        /// Creates a new button action binder with a custom button name
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="buttonName">The label for the button</param>
        public ButtonActionBinder(InputManager inputManager, string buttonName) : base(inputManager)
        {
            NextName = buttonName;
            inputManager.AddListener(this);
        }

        public override string NextName { get; } = "Button";
        public override int NumBindings { get; } = 1;
        public override bool AcceptsAxes => true;
        public override bool AcceptsButtons => true;
        public override bool AcceptsDirections => false;

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
            {
                targetGesture = new GamePadButtonGesture(inputEvent.Index);
                Advance();
            }
        }

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
            {
                targetGesture = new KeyGesture(inputEvent.Key);
                Advance();
            }
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.State == ButtonState.Pressed)
            {
                targetGesture = new MouseButtonGesture(inputEvent.Button);
                Advance();
            }
        }

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.Value > AxisThreshold)
            {
                targetGesture = new AxisButtonGesture
                {
                    Axis = new GamePadAxisGesture(inputEvent.Index)
                };
                Advance();
            }
        }

        public void ProcessEvent(MouseWheelEvent inputEvent)
        {
            targetGesture = new MouseMovementGesture(MouseAxis.Wheel) { Inverted = inputEvent.WheelDelta < 0 };
            Advance();
        }
    }
}