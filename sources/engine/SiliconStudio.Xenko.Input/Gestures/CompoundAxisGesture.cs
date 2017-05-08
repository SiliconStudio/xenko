// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// An axis made up out of multiple child axis gestures, for example a positive and negative gamepad trigger to make a full axis
    /// </summary>
    [DataContract]
    public class CompoundAxisGesture : AxisGesture
    {
        private readonly Dictionary<object, float> states = new Dictionary<object, float>();
        
        public CompoundAxisGesture()
        {
            Gestures = new TrackingCollection<AxisGesture>();
            Gestures.CollectionChanged += GesturesOnCollectionChanged;
        }
        
        // Child axis gestures
        public TrackingCollection<AxisGesture> Gestures { get; }
        
        private void GestureOnChanged(object sender, AxisGestureEventArgs axisGestureEventArgs)
        {
            if (axisGestureEventArgs.State == 0.0f)
            {
                states.Remove(sender);
            }
            else
            {
                states[sender] = axisGestureEventArgs.State;
            }

            float combinedState = 0.0f;
            foreach (var pair in states)
            {
                combinedState += pair.Value;
            }

            UpdateAxis(combinedState, axisGestureEventArgs.Device);
        }

        private void GesturesOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs args)
        {
            var axis = (AxisGesture)args.Item;

            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddChild(axis);
                    axis.Changed += GestureOnChanged;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveChild(axis);
                    axis.Changed -= GestureOnChanged;
                    break;
            }
        }
    }
}