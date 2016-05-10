// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A factory to instantiate default instance of types used for the UI.
    /// </summary>
    public static class ObjectFactory
    {
        private static readonly Dictionary<Type, IObjectFactory> RegisteredFactories = new Dictionary<Type, IObjectFactory>();

        /// <summary>
        /// Registers the factory declared with the <see cref="ObjectFactoryAttribute"/> for the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>IObjectFactory.</returns>
        /// <exception cref="System.ArgumentNullException">objectType</exception>
        /// <exception cref="System.ArgumentException">
        /// The type [{0}] for the ObjectFactoryAttribute of [{1}] was not found.ToFormat(factoryAttribute.FactoryTypeName, objectType)
        /// or
        /// The type [{0}] for the ObjectFactoryAttribute of [{1}] is not a IObjectFactory.ToFormat(factoryAttribute.FactoryTypeName, objectType)
        /// </exception>
        public static IObjectFactory RegisterFactory(Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));

            IObjectFactory factory = null;
            lock (RegisteredFactories)
            {
                var factoryAttribute = objectType.GetTypeInfo().GetCustomAttribute<ObjectFactoryAttribute>();
                if (factoryAttribute != null)
                {
                    var factoryType = AssemblyRegistry.GetType(factoryAttribute.FactoryTypeName);
                    if (factoryType == null)
                    {
                        throw new ArgumentException("The type [{0}] for the ObjectFactoryAttribute of [{1}] was not found".ToFormat(factoryAttribute.FactoryTypeName, objectType));
                    }
                    factory = Activator.CreateInstance(factoryType) as IObjectFactory;
                    if (factory == null)
                    {
                        throw new ArgumentException("The type [{0}] for the ObjectFactoryAttribute of [{1}] is not a IObjectFactory".ToFormat(factoryAttribute.FactoryTypeName, objectType));
                    }
                }

                RegisteredFactories[objectType] = factory;
            }
            return factory;
        }

        /// <summary>
        /// Registers a factory for the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>IObjectFactory.</returns>
        /// <exception cref="System.ArgumentNullException">objectType</exception>
        public static void RegisterFactory(Type objectType, IObjectFactory factory)
        {
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            lock (RegisteredFactories)
            {
                RegisteredFactories[objectType] = factory;
            }
        }

        /// <summary>
        /// Gets the factory corresponding to the given object type, if available.
        /// </summary>
        /// <param name="objectType">The object type for which to retrieve the factory.</param>
        /// <returns>The factory corresponding to the given object type if available, <c>null</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">objectType</exception>
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
        /// Finds the registered factories.
        /// </summary>
        /// <returns>List&lt;Type&gt;.</returns>
        public static List<Type> FindRegisteredFactories()
        {
            lock (RegisteredFactories)
            {
                return RegisteredFactories.Keys.ToList();
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
        /// <param name="objectType">Type of the object .</param>
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
    }
}
