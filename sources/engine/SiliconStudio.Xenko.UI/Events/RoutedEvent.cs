// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Xenko.UI.Events
{
    /// <summary>
    /// Represents and identifies a routed event and declares its characteristics.
    /// </summary>
    public abstract class RoutedEvent
    {
        /// <summary>
        /// Gets the identifying name of the routed event.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the registered owner type of the routed event.
        /// </summary>
        public Type OwnerType { get; internal set; }

        /// <summary>
        /// Gets the routing strategy of the routed event.
        /// </summary>
        public RoutingStrategy RoutingStrategy { get; internal set; }

        internal abstract Type HandlerSecondArgumentType { get; }

        internal RoutedEvent()
        {
        }
    }

    /// <summary>
    /// A routed event typed with the <see cref="RoutedEventArgs"/> it triggers.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RoutedEventArgs"/> the routed event triggers</typeparam>
    public sealed class RoutedEvent<T> : RoutedEvent where T : RoutedEventArgs
    {
        internal override Type HandlerSecondArgumentType => typeof(T);
    }
}
