// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    [DataContract]
    [Display("Axis")]
    public class AxisAction : InputAction
    {
        private readonly List<AxisGestureEventArgs> events = new List<AxisGestureEventArgs>();

        public AxisAction()
        {
            Gestures.CollectionChanged += Gestures_CollectionChanged;
        }

        /// <summary>
        /// Last state of the axis
        /// </summary>
        public float LastState { get; private set; }

        public TrackingCollection<AxisGesture> Gestures { get; } = new TrackingCollection<AxisGesture>();

        [DataMemberIgnore]
        public override IReadOnlyList<InputGesture> ReadOnlyGestures => Gestures;

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
                LastState = evt.State;
                Changed?.Invoke(this, evt);
            }
            events.Clear();
        }

        public override string ToString()
        {
            return $"Axis Action \"{MappingName}\", {nameof(LastState)}: {LastState}";
        }

        public override bool TryAddGesture(InputGesture gesture)
        {
            var item = gesture as AxisGesture;
            if (item != null)
            {
                Gestures.Add(item);
                return true;
            }

            return false;
        }

        public override void Clear()
        {
            Gestures.Clear();
        }

        protected override void OnGestureAdded(InputGesture gesture)
        {
            var axis = gesture as AxisGesture;
            axis.Changed += AxisOnChanged;
        }

        protected override void OnGestureRemoved(InputGesture gesture)
        {
            var axis = gesture as AxisGesture;
            axis.Changed -= AxisOnChanged;
        }

        private void AxisOnChanged(object sender, AxisGestureEventArgs args)
        {
            events.Add(new AxisGestureEventArgs(args.Device, args.State));
        }
    }
}