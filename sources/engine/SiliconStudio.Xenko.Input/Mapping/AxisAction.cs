// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    [DataContract]
    public class AxisAction : InputAction
    {
        private readonly List<AxisGestureEventArgs> events = new List<AxisGestureEventArgs>();
        private float lastState;
        
        /// <summary>
        /// Last state of the axis
        /// </summary>
        public float LastState => lastState;

        /// <summary>
        /// Raised when the axis state changed
        /// </summary>
        public event EventHandler<AxisGestureEventArgs> Changed;

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
            var axis = gesture as IAxisGesture;
            axis.Changed += AxisOnChanged;
        }

        protected override void OnGestureRemoved(InputGestureBase gesture)
        {
            var axis = gesture as IAxisGesture;
            axis.Changed -= AxisOnChanged;
        }

        private void AxisOnChanged(object sender, AxisGestureEventArgs args)
        {
            events.Add(new AxisGestureEventArgs(args.Device, args.State));
        }

        public override string ToString()
        {
            return $"Axis Action \"{MappingName}\", {nameof(LastState)}: {LastState}";
        }
    }
}