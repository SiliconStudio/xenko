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

        /// <summary>
        /// Pushes a tag to a component and restore it after using it. See remarks for usage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component">The component.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>PropertyTagRestore&lt;T&gt;.</returns>
        /// <remarks>
        /// This method is used to set save a property value from <see cref="ComponentBase.Tags"/>, set a new value
        /// and restore it after. The returned object must be disposed once the original value must be restored.
        /// </remarks>
        public static PropertyTagRestore<T> PushTagAndRestore<T>(this ComponentBase component, PropertyKey<T> key, T value)
        {
            // TODO: Not fully satisfied with the name and the extension point (on ComponentBase). We need to review this a bit more
            var restorer = new PropertyTagRestore<T>(component, key);
            component.Tags.Set(key, value);
            return restorer;
        }

        /// <summary>
        /// Struct PropertyTagRestore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct PropertyTagRestore<T> : IDisposable
        {
            private readonly ComponentBase container;

            private readonly PropertyKey<T> key;

            private readonly T previousValue;

            public PropertyTagRestore(ComponentBase container, PropertyKey<T> key)
                : this()
            {
                if (container == null) throw new ArgumentNullException("container");
                if (key == null) throw new ArgumentNullException("key");
                this.container = container;
                this.key = key;
                previousValue = container.Tags.Get(key);
            }

            public void Dispose()
            {
                // Restore the value
                container.Tags.Set(key, previousValue);
            }
        }
    }
}