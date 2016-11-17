// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
    public class CompoundAxisGesture : AxisGestureBase
    {
        private Dictionary<object, float> states = new Dictionary<object, float>();
        private TrackingCollection<IAxisGesture> gestures;
        
        public CompoundAxisGesture()
        {
            gestures = new TrackingCollection<IAxisGesture>();
            gestures.CollectionChanged += GesturesOnCollectionChanged;
        }
        
        // Child axis gestures
        public TrackingCollection<IAxisGesture> Gestures
        {
            get { return gestures; }
            set
            {
                // Replace whole collection of gestures
                foreach (var gesture in gestures)
                {
                    states.Remove(gesture);
                    gesture.Changed -= GestureOnChanged;
                    RemoveChild(gesture);
                }
                gestures = value;
                foreach (var gesture in gestures)
                {
                    AddChild(gesture);
                    gesture.Changed += GestureOnChanged;
                }
                gestures.CollectionChanged += GesturesOnCollectionChanged;
            }
        }
        
        private void GestureOnChanged(object sender, AxisGestureEventArgs axisGestureEventArgs)
        {
            if (axisGestureEventArgs.State == 0.0f)
                states.Remove(sender);
            else
                states[sender] = axisGestureEventArgs.State;

            float combinedState = 0.0f;
            foreach (var pair in states)
            {
                combinedState += pair.Value;
            }
            UpdateAxis(combinedState, axisGestureEventArgs.Device);
        }

        private void GesturesOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs args)
        {
            var axis = ((IAxisGesture)args.Item);
            if(axis == null) throw new InvalidOperationException("Item must be an IAxisGesture");
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