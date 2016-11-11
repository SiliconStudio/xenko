// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// Base class of <see cref="IInputGesture"/>, every input gesture should inherit from this class if you want to use them with <see cref="InputActionMapping"/>
    /// </summary>
    [DataContract]
    public abstract class InputGesture : IInputGesture
    {
        internal InputActionMapping ActionMapping;
        internal InputAction Action;
        private readonly List<InputGesture> childGestures = new List<InputGesture>();
        
        /// <param name="elapsedTime"></param>
        /// <inheritdoc />
        public virtual void Reset(TimeSpan elapsedTime)
        {
        }

        internal void OnAdded()
        {
            ActionMapping.AddInputGesture(this);
            foreach (var child in childGestures)
            {
                child.ActionMapping = ActionMapping;
                child.Action = Action;
                child.OnAdded();
            }
        }

        internal void OnRemoved()
        {
            ActionMapping.RemoveInputGesture(this);
            ActionMapping = null;
            Action = null;
            foreach (var child in childGestures)
            {
                child.OnRemoved();
            }
        }

        /// <summary>
        /// Registers a child gesture of this gesture
        /// </summary>
        /// <param name="child">The child to add</param>
        protected void AddChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGesture;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGesture");
            childGestures.Add(gesture);
            if (ActionMapping != null)
                gesture.OnAdded();
        }

        /// <summary>
        /// Removes a child gesture from this gesture
        /// </summary>
        /// <param name="child">The child to remove</param>
        protected void RemoveChild(IInputGesture child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            var gesture = child as InputGesture;
            if (gesture == null) throw new InvalidOperationException("Gesture does not inherit from InputGesture");
            if (ActionMapping != null)
                gesture.OnRemoved();
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