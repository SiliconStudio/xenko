// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Input
{
    public interface IInputEventListener
    {
    }

    /// <summary>
    /// Interface for classes that want to listen to input event of a certain type
    /// </summary>
    /// <typeparam name="TEventType">The type of <see cref="InputEvent"/> that will be sent to this event listener</typeparam>
    public interface IInputEventListener<TEventType> : IInputEventListener where TEventType : InputEvent
    {
        /// <summary>
        /// Processes a new input event
        /// </summary>
        /// <param name="inputEvent">the input event</param>
        void ProcessEvent(TEventType inputEvent);
    }
}