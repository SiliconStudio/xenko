// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Input.Mapping;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Base class for implementing <see cref="IInputGesture"/>, every input gesture should inherit from this class
    /// </summary>
    [DataContract]
    public abstract class InputGestureBase : IInputGesture
    {
        protected InputGestureBase Parent;
        protected internal InputManager InputManager;
        private readonly List<InputGestureBase> childGestures = new List<InputGestureBase>();
        
        public virtual void PreUpdate(TimeSpan elapsedTime)
        {
            foreach(var child in childGestures)
                child.PreUpdate(elapsedTime);
        }
        
        public virtual void Update(TimeSpan elapsedTime)
        {
            foreach (var child in childGestures)
                child.Update(elapsedTime);
        }

        /// <summary>
        /// Fills a collection with all gestures in this tree
        /// </summary>
        public void GetGesturesRecursive(ICollection<IInputGesture> gestures)
        {
            gestures.Add(this);
            foreach (var child in childGestures)
            {
                child.GetGesturesRecursive(gestures);
            }
        }
        
        /// <summary>
        /// Registers a child gesture of this gesture
        /// </summary>
        /// <param name="child">The child to add</param>
        protected void AddChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGestureBase;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGestureBase");
            gesture.Parent = this;
            childGestures.Add(gesture);

            // Call initialize on new gesture
            if (InputManager != null)
                gesture.OnAdded();
        }
        
        /// <summary>
        /// Called when the gesture or it's parent is attached to the <see cref="InputManager"/>
        /// </summary>
        protected internal virtual void OnAdded()
        {
            var eventListener = this as IInputEventListener;
            if (eventListener != null) InputManager.AddListener(eventListener);

            foreach (var child in childGestures)
            {
                child.InputManager = InputManager;
                child.OnAdded();
            }
        }

        /// <summary>
        /// Called when the gesture or it's parent is remove from the <see cref="InputManager"/>
        /// </summary>
        protected internal virtual void OnRemoved()
        {
            var eventListener = this as IInputEventListener;
            if (eventListener != null) InputManager.RemoveListener(eventListener);

            InputManager = null;
            foreach (var child in childGestures)
            {
                child.OnRemoved();
            }
        }

        /// <summary>
        /// Removes a child gesture from this gesture
        /// </summary>
        /// <param name="child">The child to remove</param>
        protected void RemoveChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGestureBase;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGestureBase");
            gesture.Parent = null;
            childGestures.Remove(gesture);
        }

        /// <summary>
        /// Replaces an old child gesture with a new one (can be used in property setters)
        /// </summary>
        /// <param name="oldChild">A child to remove (if not null)</param>
        /// <param name="newChild">A child to add (if not null)</param>
        protected void UpdateChild(IInputGesture oldChild, IInputGesture newChild)
        {
            if (oldChild != null)
                RemoveChild(oldChild);
            if (newChild != null)
                AddChild(newChild);
        }
    }
}