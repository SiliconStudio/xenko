// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Extensions for <see cref="IComponent"/>.
    /// </summary>
    public static class ComponentBaseExtensions
    {
        /// <summary>
        /// Keeps a component alive by adding it to a container.
        /// </summary>
        /// <typeparam name="T">A component</typeparam>
        /// <param name="thisArg">The component to keep alive.</param>
        /// <param name="container">The container that will keep a reference to the component.</param>
        /// <returns>The same component instance</returns>
        public static void RemoveKeepAliveBy<T>(this T thisArg, ICollectorHolder container) where T : IReferencable
        {
            if (ReferenceEquals(thisArg, null))
                return;
            container.Collector.Remove(thisArg);
        }

        /// <summary>
        /// Keeps a component alive by adding it to a container.
        /// </summary>
        /// <typeparam name="T">A component</typeparam>
        /// <param name="thisArg">The component to keep alive.</param>
        /// <param name="container">The container that will keep a reference to the component.</param>
        /// <returns>The same component instance</returns>
        public static T KeepAliveBy<T>(this T thisArg, ICollectorHolder container) where T : IReferencable
        {
            if (ReferenceEquals(thisArg, null))
                return thisArg;
            return container.Collector.Add(thisArg);
        }

        /// <summary>
        /// Keeps a component alive by adding it to a container.
        /// </summary>
        /// <typeparam name="T">A component</typeparam>
        /// <param name="thisArg">The component to keep alive.</param>
        /// <param name="collector">The collector.</param>
        /// <returns>The same component instance</returns>
        public static T KeepAliveBy<T>(this T thisArg, ObjectCollector collector) where T : IReferencable
        {
            if (ReferenceEquals(thisArg, null))
                return thisArg;
            return collector.Add(thisArg);
        }

        /// <summary>
        /// Pins this component as a new reference.
        /// </summary>
        /// <typeparam name="T">A component</typeparam>
        /// <param name="thisArg">The component to add a reference to.</param>
        /// <returns>This component.</returns>
        /// <remarks>This method is equivalent to call <see cref="IReferencable.AddReference"/> and return this instance.</remarks>
        public static T KeepReference<T>(this T thisArg) where T : IReferencable
        {
            if (ReferenceEquals(thisArg, null))
                return thisArg;
            thisArg.AddReference();
            return thisArg;
        }

        /// <summary>
        /// Keeps a component alive by adding it to a container.
        /// </summary>
        /// <typeparam name="T">A component</typeparam>
        /// <param name="thisArg">The component to keep alive.</param>
        /// <param name="container">The container that will keep a reference to the component.</param>
        /// <returns>The same component instance</returns>
        public static T DisposeBy<T>(this T thisArg, ICollectorHolder container) where T : IDisposable
        {
            if (ReferenceEquals(thisArg, null))
                return thisArg;
            return container.Collector.Add(thisArg);
        }
    }
}