// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Specialized;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An object that can respond to actions from various input gestures (keyboard,mouse,touch,gamepad,etc.)
    /// </summary>
    public abstract class InputAction
    {
        internal InputActionMapping ActionMapping;

        protected InputAction()
        {
            Gestures.CollectionChanged += Gestures_CollectionChanged;
        }

        /// <summary>
        /// The gestures that are used for this action
        /// </summary>
        public TrackingCollection<IInputGesture> Gestures { get; } = new TrackingCollection<IInputGesture>();

        /// <summary>
        /// The name of the action, as registered in the action mapping
        /// </summary>
        public string MappingName { get; internal set; }

        /// <summary>
        /// Updates the input action, raising events whenever something changed
        /// </summary>
        public abstract void Update();

        private void Gestures_CollectionChanged(object sender, TrackingCollectionChangedEventArgs e)
        {
            // Handles adding/removing new gestures to/from the action mapping when this action is registered as well
            var gesture = e.Item as InputGesture;
            if (gesture == null) throw new InvalidOperationException("New item does not inherit from InputGesture");
            if (ActionMapping == null) return;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    gesture.ActionMapping = ActionMapping;
                    gesture.OnAdded();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    gesture.OnRemoved();
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Gestures collection was modified but the action was not supported by the system.");
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}