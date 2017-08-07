// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.UI.Events
{
    internal abstract class RoutedEventHandlerInfo : IEquatable<RoutedEventHandlerInfo>
    {
        protected RoutedEventHandlerInfo(bool handledEventToo)
        {
            HandledEventToo = handledEventToo;
        }

        public bool HandledEventToo { get; }

        public abstract Delegate Handler { get; }

        public abstract void Invoke(object sender, RoutedEventArgs args);

        /// <inheritdoc />
        public bool Equals(RoutedEventHandlerInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Handler, other.Handler);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as RoutedEventHandlerInfo);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Handler.GetHashCode();
        }

        public static bool operator ==(RoutedEventHandlerInfo left, RoutedEventHandlerInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RoutedEventHandlerInfo left, RoutedEventHandlerInfo right)
        {
            return !Equals(left, right);
        }
    }

    internal sealed class RoutedEventHandlerInfo<T> : RoutedEventHandlerInfo where T : RoutedEventArgs
    {
        public RoutedEventHandlerInfo(EventHandler<T> routedEventHandler, bool handledEventToo = false)
            : base(handledEventToo)
        {
            RoutedEventHandler = routedEventHandler;
        }

        /// <inheritdoc/>
        public override Delegate Handler => RoutedEventHandler;

        public EventHandler<T> RoutedEventHandler { get; }

        /// <inheritdoc/>
        public override void Invoke(object sender, RoutedEventArgs args)
        {
            RoutedEventHandler(sender, (T)args);
        }
    }
}
