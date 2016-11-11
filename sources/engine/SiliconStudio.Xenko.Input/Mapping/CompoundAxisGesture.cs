// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An axis made up out of multiple child axis gestures
    /// </summary>
    [DataContract]
    public class CompoundAxisGesture : ScalableInputGesture, IAxisGesture
    {
        private TrackingCollection<IAxisGesture> gestures;

        public CompoundAxisGesture()
        {
            gestures = new TrackingCollection<IAxisGesture>();
            gestures.CollectionChanged += GesturesOnCollectionChanged;
        }
        
        public TrackingCollection<IAxisGesture> Gestures
        {
            get { return gestures; }
            set
            {
                // Replace whole collection of gestures
                foreach (var gesture in gestures)
                {
                    RemoveChild(gesture);
                }
                gestures = value;
                foreach (var gesture in gestures)
                {
                    AddChild(gesture);
                }
                gestures.CollectionChanged += GesturesOnCollectionChanged;
            }
        }

        [DataMemberIgnore]
        public float Axis
        {
            get
            {
                // Evaluate to largest absolute value
                float largest = 0.0f;
                foreach (var gesture in Gestures)
                {
                    var value = gesture.Axis;
                    if (Math.Abs(value) > Math.Abs(largest))
                    {
                        largest = value;
                    }
                }
                return largest;
            }
        }

        [DataMemberIgnore]
        public bool IsRelative
        {
            get
            {
                if (gestures.Count > 0)
                    return gestures[0].IsRelative;
                return false;
            }
        }

        public override void Reset(TimeSpan elapsedTime)
        {
            base.Reset(elapsedTime);
            foreach (var gesture in gestures)
                gesture.Reset(elapsedTime);
        }

        private void GesturesOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddChild((IInputGesture)args.Item);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveChild((IInputGesture)args.Item);
                    break;
            }
        }
    }
}