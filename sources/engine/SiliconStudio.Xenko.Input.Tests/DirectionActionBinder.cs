// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Generates gestures mapping to direction actions
    /// </summary>
    public class DirectionActionBinder : AxisActionBinder, IInputEventListener<GamePadPovControllerEvent>
    {
        private FourWayGesture fourWayGesture;
        protected bool canBindButtons = true;

        /// <summary>
        /// Creates a new direction action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        public DirectionActionBinder(InputManager inputManager) : base(inputManager)
        {
            names = new[] { "Right", "Left", "Up", "Down" };
            targetGesture = fourWayGesture = new FourWayGesture();
        }

        /// <summary>
        /// Creates a new direction action binder with custom names for each direction
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// <param name="right">Label for right</param>
        /// <param name="left">Label for left</param>
        /// <param name="up">Label for up</param>
        /// <param name="down">Label for down</param>
        public DirectionActionBinder(InputManager inputManager, string right, string left, string up, string down) : base(inputManager)
        {
            names = new[] { right, left, up, down };
            targetGesture = fourWayGesture = new FourWayGesture();
        }

        public override string NextName => names[MathUtil.Clamp(Index, 0, 3)];
        public override int NumBindings { get; } = 4;
        public override bool AcceptsAxes => Index == 0 || !canBindButtons;
        public override bool AcceptsButtons => canBindButtons;
        public override bool AcceptsDirections => Index == 0;
        
        protected override TwoWayGesture AsTwoWayGesture()
        {
            if (AcceptsButtons)
            {
                if (fourWayGesture.X == null)
                {
                    // Always bind four buttons at once
                    fourWayGesture.X = new TwoWayGesture();
                    fourWayGesture.Y = new TwoWayGesture();
                }
                return ((Index < 2) ? fourWayGesture.X : fourWayGesture.Y) as TwoWayGesture;
            }
            return null;
        }

        protected override void TryBindAxis(IAxisGesture axis)
        {
            // Filter out duplicate axes
            if (usedGestures.Contains(axis)) return;

            if (Index == 0)
            {
                fourWayGesture.X = axis;
                usedGestures.Add(axis);
                canBindButtons = false; // Don't allow buttons after binding an axis
                Advance(2);
            }
            else if (AcceptsAxes)
            {
                fourWayGesture.Y = axis;
                Advance(2);
            }
        }

        public void ProcessEvent(GamePadPovControllerEvent inputEvent)
        {
            if(AcceptsDirections)
            {
                targetGesture = new GamePadPovGesture(inputEvent.Index) {ControllerIndex = inputEvent.GamePad.Index};
                Advance(4);
            }
        }
    }
}