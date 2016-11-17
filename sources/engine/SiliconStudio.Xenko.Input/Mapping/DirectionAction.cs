// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a direction and velocity
    /// </summary>
    [DataContract]
    public class DirectionAction : InputAction
    {
        private readonly List<DirectionGestureEventArgs> events = new List<DirectionGestureEventArgs>();
        private Vector2 lastState;

        /// <summary>
        /// Raised when the direction state changed
        /// </summary>
        public event EventHandler<DirectionGestureEventArgs> Changed;

        public Vector2 LastState => lastState;

        public override void Update(TimeSpan deltaTime)
        {
            base.Update(deltaTime);

            // Only send the last event
            if (events.Count > 0)
            {
                var evt = events.Last();
                lastState = evt.State;
                Changed?.Invoke(this, evt);
            }
            events.Clear();
        }

        protected override void OnGestureAdded(InputGestureBase gesture)
        {
            var direction = gesture as IDirectionGesture;
            direction.Changed += DirectionOnChanged;
        }

        protected override void OnGestureRemoved(InputGestureBase gesture)
        {
            var direction = gesture as IDirectionGesture;
            direction.Changed -= DirectionOnChanged;
        }

        private void DirectionOnChanged(object sender, DirectionGestureEventArgs args)
        {
            events.Add(new DirectionGestureEventArgs(args.Device, args.State));
        }

        public override string ToString()
        {
            return $"Direction Action \"{MappingName}\", {nameof(LastState)}: {LastState}";
        }
    }
}