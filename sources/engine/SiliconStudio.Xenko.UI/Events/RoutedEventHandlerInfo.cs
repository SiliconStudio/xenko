// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Xenko.UI.Events
{
    internal abstract class RoutedEventHandlerInfo
    {
        public bool HandledEventToo { get; private set; }

        public abstract void Invoke(object sender, RoutedEventArgs args);

        public abstract Delegate Handler { get; }

        protected RoutedEventHandlerInfo(bool handledEventToo)
        {
            HandledEventToo = handledEventToo;
        }

        public override bool Equals(object obj)
        {
            var castedObj = (RoutedEventHandlerInfo)obj;
            return Handler.Equals(castedObj.Handler);
        }

        public override int GetHashCode()
        {
            return Handler.GetHashCode();
        }
    }

    internal class RoutedEventHandlerInfo<T> : RoutedEventHandlerInfo where T : RoutedEventArgs
    {
        public EventHandler<T> RoutedEventHandler { get; }

        public RoutedEventHandlerInfo(EventHandler<T> routedEventHandler, bool handledEventToo = false)
            : base(handledEventToo)
        {
            RoutedEventHandler = routedEventHandler;
        }

        public override void Invoke(object sender, RoutedEventArgs args)
        {
            RoutedEventHandler(sender, (T)args);
        }

        public override Delegate Handler => RoutedEventHandler;
    }
}
