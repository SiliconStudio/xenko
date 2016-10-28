// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Xenko.Input.Tests
{
    /// <summary>
    /// Receives a single or chain of inputs in order to create a <see cref="IInputGesture"/> that represents the given input
    /// </summary>
    public abstract class ActionBinder : IInputEventListener, IDisposable
    {
        protected IInputGesture targetGesture;
        protected InputManager inputManager;

        protected ActionBinder(InputManager inputManager)
        {
            this.inputManager = inputManager;
        }

        public void Dispose()
        {
            if (!Done) inputManager.RemoveListener(this);
        }

        /// <summary>
        /// Provides a name for the next element that will be bound
        /// </summary>
        public abstract string NextName { get; }

        /// <summary>
        /// <c>true</c> if binding has received all the required inputs to bind the action
        /// </summary>
        public bool Done { get; private set; }

        /// <summary>
        /// Index of the current input
        /// </summary>
        public int Index { get; private set; } = 0;

        /// <summary>
        /// <c>true</c> if this binder accepts axes input events, such as mouse X axis or gamepad axis 1
        /// </summary>
        public virtual bool AcceptsAxes { get; private set; } = true;

        /// <summary>
        /// <c>true</c> if this binder accepts button input events, such as keyboard keys and mouse buttons
        /// </summary>
        public virtual bool AcceptsButtons { get; private set; } = true;

        /// <summary>
        /// <c>true</c> if this binder accepts combinded directional input events, such as mouse X,Y movement and gamepad pov controller
        /// </summary>
        public virtual bool AcceptsDirections { get; private set; } = true;

        /// <summary>
        /// The number of bindings
        /// </summary>
        public abstract int NumBindings { get; }

        /// <summary>
        /// The generated gesture
        /// </summary>
        public IInputGesture TargetGesture => targetGesture;

        /// <summary>
        /// Moves the input being detected by a given amount (default = 1). 
        /// If the number of inputs has reached <see cref="NumBindings"/>, <see cref="Done"/> will be set to true and the action will unbind itself from the input manager
        /// </summary>
        /// <param name="amount">The number of inputs to advance</param>
        protected void Advance(int amount = 1)
        {
            if (!Done)
            {
                Index += amount;
                Done = Index >= NumBindings;
                if (Done)
                {
                    inputManager.RemoveListener(this);
                    Index = NumBindings - 1;
                }
            }
        }
    }
}