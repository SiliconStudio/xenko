// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Generates gestures mapping to direction actions, bindings are made in the order Right/Left/Up/Down (or first stick movement binds to the positive direction of one axis)
    /// </summary>
    public class DirectionActionBinder : AxisActionBinder, IInputEventListener<PovControllerEvent>
    {
        /// <summary>
        /// Backing field of <see cref="AcceptsButtons"/>
        /// </summary>
        protected bool CanBindButtons = true;
        protected readonly FourWayGesture TargetFourWayGesture;

        /// <summary>
        /// Creates a new direction action binder
        /// </summary>
        /// <param name="inputManager">The <see cref="InputManager"/> to monitor for input events</param>
        /// /// <param name="usedGestures">A set of already used gesture that are filtered out from the input, can be null</param>
        public DirectionActionBinder(InputManager inputManager, HashSet<IInputGesture> usedGestures = null) : base(inputManager, usedGestures)
        {
            // A direction is always generated from a four way gesture
            TargetGesture = TargetFourWayGesture = new FourWayGesture();
        }
        
        public override int BindingCount { get; } = 4;

        public override bool AcceptsAxes => Index == 0 || !CanBindButtons;

        public override bool AcceptsButtons => CanBindButtons;

        public override bool AcceptsDirections => Index == 0;
        
        public void ProcessEvent(PovControllerEvent inputEvent)
        {
            if (AcceptsDirections)
            {
                var target = new PovControllerGesture(inputEvent.Index, inputEvent.GameController.Id);

                // Filter out duplicate pov gestures
                if (UsedGestures.Contains(target)) return;

                TargetGesture = target;
                Advance(4);

                UsedGestures.Add(target);
            }
        }

        protected override TwoWayGesture AsTwoWayGesture()
        {
            if (AcceptsButtons)
            {
                if (TargetFourWayGesture.X == null)
                {
                    // Always bind four buttons at once
                    TargetFourWayGesture.X = new TwoWayGesture();
                    TargetFourWayGesture.Y = new TwoWayGesture();
                }

                return ((Index < 2) ? TargetFourWayGesture.X : TargetFourWayGesture.Y) as TwoWayGesture;
            }
            return null;
        }

        protected override void TryBindAxis(IAxisGesture axis, bool isBidirectional)
        {
            // Filter out duplicate axes
            if (UsedGestures.Contains(axis))
                return;

            // Don't bind triggers to directional axes
            if (!isBidirectional)
                return;

            if (Index == 0)
            {
                TargetFourWayGesture.X = axis;
                CanBindButtons = false; // Don't allow buttons after binding an axis
                Advance(2);
            }
            else if (AcceptsAxes)
            {
                TargetFourWayGesture.Y = axis;
                Advance(2);
            }

            UsedGestures.Add(axis);
        }
    }
}