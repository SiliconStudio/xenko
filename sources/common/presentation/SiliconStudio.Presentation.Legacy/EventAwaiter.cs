// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

namespace SiliconStudio.Presentation.Legacy
{
    public class EventAwaiter : EventAwaiter<object>
    {
        public EventAwaiter(object instance, string eventName)
            : base(instance, eventName)
        {
        }
    }

    public class EventAwaiter<T>
    {
        public T Instance { get; private set; }
        public string EventName { get; private set; }
        public bool IsCompleted { get; protected set; }
        private IDisposable subscription;
        private Action awaitContinuation;
        private EventArgs eventArgs;

        public EventAwaiter(T instance, string eventName)
        {
            Instance = instance;
            EventName = eventName;
        }

        protected virtual bool NeedEventHook { get { return true; } }
        protected virtual void OnEvent(object sender, EventArgs e, Action continuation) { continuation(); }

        private void OnEvent(object sender, EventArgs e)
        {
            eventArgs = e;

            OnEvent(sender, e, () =>
            {
                IsCompleted = true;
                awaitContinuation();
                if (subscription != null)
                {
                    subscription.Dispose();
                    subscription = null;
                }
            });
        }

        public void OnCompleted(Action continuation)
        {
            if (NeedEventHook == false)
            {
                IsCompleted = true;
                continuation();
            }
            else
            {
                awaitContinuation = continuation;
                // setup event hook only when NeedEventHook returned true or when
                // it returned false but OnCompltedOverride returned false too
                subscription = new AnonymousEventHook(Instance, EventName, OnEvent);
            }
        }

        public virtual EventArgs GetResult()
        {
            return eventArgs;
        }

        public virtual EventAwaiter<T> GetAwaiter()
        {
            return this;
        }
    }

    public class LoadedEventAwaiter : EventAwaiter<FrameworkElement>
    {
        public LoadedEventAwaiter(FrameworkElement element)
            : base(element, "Loaded")
        {
        }

        protected override bool NeedEventHook
        {
            get
            {
                return Instance.IsLoaded == false;
            }
        }
    }
}
