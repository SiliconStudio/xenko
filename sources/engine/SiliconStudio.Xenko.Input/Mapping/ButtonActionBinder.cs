// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Generates gestures mapping to button actions
    /// </summary>
    public class ButtonActionBinder : ActionBinder,
        IInputEventListener<KeyEvent>,
        IInputEventListener<MouseButtonEvent>,
        IInputEventListener<GameControllerButtonEvent>,
        IInputEventListener<GamePadButtonEvent>,
        IInputEventListener<GameControllerAxisEvent>,
        IInputEventListener<GamePadAxisEvent>
    {
        /// <summary>
        /// Creates a new button action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="usedGestures">A set of already used gesture that are filtered out from the input, can be null</param>
        public ButtonActionBinder(InputManager inputManager, HashSet<InputGesture> usedGestures = null) : base(inputManager, usedGestures)
        {
            inputManager.AddListener(this);
        }

        /// <summary>
        /// The threshold that is used to trigger using axes as buttons
        /// </summary>
        public float AxisThreshold { get; set; } = 0.9f;

        public override int BindingCount { get; } = 1;

        public override bool AcceptsAxes => true;

        public override bool AcceptsButtons => true;

        public override bool AcceptsDirections => false;

        public void ProcessEvent(KeyEvent inputEvent)
        {
            if (inputEvent.IsDown)
            {
                TryBindButton(new KeyGesture(inputEvent.Key));
            }
        }

        public void ProcessEvent(MouseButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
            {
                TryBindButton(new MouseButtonGesture(inputEvent.Button));
            }
        }

        public void ProcessEvent(GameControllerButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
            {
                TryBindButton(new GameControllerButtonGesture(inputEvent.Index, inputEvent.GameController.Id));
            }
        }

        public void ProcessEvent(GameControllerAxisEvent inputEvent)
        {
            if (inputEvent.Value > AxisThreshold)
            {
                TryBindButton(new AxisButtonGesture
                {
                    Axis = new GameControllerAxisGesture(inputEvent.Index, inputEvent.GameController.Id)
                });
            }
        }

        public void ProcessEvent(GamePadButtonEvent inputEvent)
        {
            if (inputEvent.IsDown)
            {
                TryBindButton(new GamePadButtonGesture(inputEvent.Button, inputEvent.GamePad.Index));
            }
        }

        public void ProcessEvent(GamePadAxisEvent inputEvent)
        {
            if (inputEvent.Value > AxisThreshold)
            {
                TryBindButton(new AxisButtonGesture
                {
                    Axis = new GamePadAxisGesture(inputEvent.Axis, inputEvent.GamePad.Index)
                });
            }
        }

        protected void TryBindButton(ButtonGesture button)
        {
            // Filter out duplicate buttons
            if (UsedGestures.Contains(button)) return;

            TargetGesture = button;

            UsedGestures.Add(button);
            Advance();
        }
    }
}