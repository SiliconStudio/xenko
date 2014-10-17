// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Windows;

using SiliconStudio.Core;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Represents an event subscription.
    /// </summary>
    public abstract class EventHook : IDisposable
    {
        // represent the event subscription
        private readonly IDisposable removeHandler;
        // local method event handler
        private object internalHandler;

        /// <summary>
        /// Instanciates an <c>EventHook</c> instance.
        /// </summary>
        /// <param name="instance">Instance on which to subscribe to an event.</param>
        /// <param name="eventName">The event to subscribe to, represented by its name.</param>
        public EventHook(object instance, string eventName)
        {
            // ensure instances parameter is not null
            if (instance == null)
                throw new ArgumentNullException("instance");

            // ensure eventName is not null
            if (eventName == null)
                throw new ArgumentNullException("eventName");

            // subscribe each instance to the event
            // (call ToArray to enforce subscription process to run)
            removeHandler = Subscribe(instance, eventName);
        }

        // subscribe the 'handler' to the 'eventName' on the 'instance'
        // this method is used to intercept event subscription and redirect it to abstract OnEvent method
        private IDisposable Subscribe(object instance, string eventName)
        {
            // retrieve the type of the instance
            Type type = instance.GetType();

            // retrieve the underlying event information through refelction
            EventInfo eventInfo = type.GetEvent(eventName);
            // ensure the event specified by name exists (ensure spell miss)
            if (eventInfo == null)
                throw new InvalidOperationException(string.Format("Object of type '{0}' does not have an event named '{1}'", type.FullName, eventName));

            // get the add method (+= operator)
            MethodInfo addMethod = eventInfo.GetAddMethod();
            try
            {
                ParameterInfo[] parameters = addMethod.GetParameters();
                if (parameters.Length != 1)
                    throw new InvalidOperationException(string.Format("Invalid number of parameters in add method of event '{0}'", eventName));

                // creates a local method delegate and stores its instance for later use
                internalHandler = ProduceHandler(parameters[0].ParameterType);

                // subscribe the handler given as parameter on the given instance
                addMethod.Invoke(instance, new[] { internalHandler });

                // get the remove method (-= operator)
                MethodInfo removeMethod = eventInfo.GetRemoveMethod();

                // creates and returns an IDisposable that unsubscribes on dispose
                return new AnonymousDisposable(() => removeMethod.Invoke(instance, new[] { internalHandler }));
            }
            catch
            {
                // impossible to subscribe to non-standard events
                // should rethrow a more explicit exception ?
                return new NullDisposable();
            }
        }

        private object ProduceHandler(Type type)
        {
            object result = null;

            if (type == typeof(EventHandler))
                result = new EventHandler(HookedEventFunc);
            else if (type == typeof(RoutedEventHandler))
                result = new RoutedEventHandler(HookedEventFunc);
            else if (type.IsSubclassOf(typeof(MulticastDelegate)))
                result = Delegate.CreateDelegate(type, this, "HookedEventFunc", false, false);

            if (result != null)
                return result;

            throw new NotSupportedException("Generic EventHandler not supported");
        }

        // local method used to intercept events and redirect to abstract OnEvent method
        private void HookedEventFunc(object sender, EventArgs e)
        {
            // ensure object is not disposed
            if (isDisposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            // call abstract method
            OnEvent(sender, e);
        }

        /// <summary>
        /// When overridden, it is raised when the subscribed event is fired on the instance.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">The weakly typed <c>EventArgs</c> produced by the event source.</param>
        protected abstract void OnEvent(object sender, EventArgs e);

        private bool isDisposed;
        public void Dispose()
        {
            // ensure disposal safety
            if (isDisposed)
                return;

            // mark object as disposed
            isDisposed = true;

            // unsubscribe the handler subscription by disposing
            removeHandler.Dispose();
        }
    }

    /// <summary>
    /// Represents an <c>EventHook</c> that delegates task to another method.
    /// </summary>
    public class AnonymousEventHook : EventHook
    {
        // stores the user-defined handler mehtod
        private readonly Action<object, EventArgs> externalHandler;

        /// <summary>
        /// Initializes an <c>AnonymousEventHook</c> instance.
        /// </summary>
        /// <param name="instance">Instance on which to subscribe to event.</param>
        /// <param name="eventName">The event to subscribe to, represented by its name.</param>
        /// <param name="handler">The method that is executed when event is fired.</param>
        public AnonymousEventHook(object instance, string eventName, Action<object, EventArgs> handler)
            : base(instance, eventName)
        {
            // ensure eventName is not null
            if (eventName == null)
                throw new ArgumentNullException("eventName");

            // ensure handler parameter is no null
            if (handler == null)
                throw new ArgumentNullException("handler");

            // store the external method
            externalHandler = handler;
        }

        // method called when event is fired
        protected override void OnEvent(object sender, EventArgs e)
        {
            // redirect call to external user-defiend method
            externalHandler(sender, e);
        }
    }
}
