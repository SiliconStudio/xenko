// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A registry of <see cref="IObjectFactory"/> used to instantiate instances of types used at design-time.
    /// </summary>
    public static class ObjectFactoryRegistry
    {
        private static readonly Dictionary<Type, IObjectFactory> RegisteredFactories = new Dictionary<Type, IObjectFactory>();

        /// <summary>
        /// Gets the factory corresponding to the given object type, if available.
        /// </summary>
        /// <param name="objectType">The object type for which to retrieve the factory.</param>
        /// <returns>The factory corresponding to the given object type if available, <c>null</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">objectType</exception>
        public static IObjectFactory GetFactory(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            lock (RegisteredFactories)
            {
                IObjectFactory factory;
                RegisteredFactories.TryGetValue(objectType, out factory);
                return factory;
            }
        }

        /// <summary>
        /// Creates a default instance for an object type.
        /// </summary>
        /// <typeparam name="T">Type of the object to create</typeparam>
        /// <returns>A new instance of T</returns>
        public static T NewInstance<T>()
        {
            return (T)NewInstance(typeof(T));
        }

        /// <summary>
        /// Creates a default instance for an object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>A new default instance of an object.</returns>
        public static object NewInstance(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            IObjectFactory factory;

            lock (RegisteredFactories)
            {
                if (!RegisteredFactories.TryGetValue(objectType, out factory))
                {
                    factory = RegisterFactory(objectType);
                }
            }

            // If no registered factory, creates directly the asset
            return factory != null ? factory.New(objectType) : Activator.CreateInstance(objectType);
        }

        private static IObjectFactory RegisterFactory(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            IObjectFactory factory = null;
            lock (RegisteredFactories)
            {
                var factoryAttribute = objectType.GetTypeInfo().GetCustomAttribute<ObjectFactoryAttribute>();
                if (factoryAttribute != null)
                {
                    factory = Activator.CreateInstance(factoryAttribute.FactoryType) as IObjectFactory;
                }

                RegisteredFactories[objectType] = factory;
            }

            return factory;
        }
    }
}
