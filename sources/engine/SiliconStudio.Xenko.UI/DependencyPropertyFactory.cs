// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.UI
{
    public static class DependencyPropertyFactory
    {
        /// <summary>
        /// Registers a dependency property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="ownerType">The type that is registering the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="metadatas">The metadatas.</param>
        /// <returns></returns>
        public static PropertyKey<T> Register<T>(string name, Type ownerType, T defaultValue, params PropertyKeyMetadata[] metadatas)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
            if (metadatas == null) throw new ArgumentNullException(nameof(metadatas));

            if (!typeof(UIElement).IsAssignableFrom(ownerType))
                throw new ArgumentException($"{ownerType.FullName} must be a subclass of {nameof(UIElement)}", nameof(ownerType));

            var allMetadataCount = metadatas.Length + 2;
            var allMetadatas = new PropertyKeyMetadata[allMetadataCount];
            allMetadatas[0] = DefaultValueMetadata.Static(defaultValue);
            allMetadatas[1] = DependencyPropertyKeyMetadata.Default;
            Array.Copy(metadatas, 0, allMetadatas, allMetadataCount - metadatas.Length, metadatas.Length);

            return new PropertyKey<T>(name, ownerType, allMetadatas);
        }

        /// <summary>
        /// Registers an attached dependency property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="ownerType">The type that is registering the property.</param>
        /// <param name="defaultValue">The default value of the property.</param>
        /// <param name="metadatas">The metadatas.</param>
        /// <returns></returns>
        public static PropertyKey<T> RegisterAttached<T>(string name, Type ownerType, T defaultValue, params PropertyKeyMetadata[] metadatas)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (ownerType == null) throw new ArgumentNullException(nameof(ownerType));
            if (metadatas == null) throw new ArgumentNullException(nameof(metadatas));

            if (!typeof(UIElement).IsAssignableFrom(ownerType))
                throw new ArgumentException($"{ownerType.FullName} must be a subclass of {nameof(UIElement)}", nameof(ownerType));

            var allMetadataCount = metadatas.Length + 2;
            var allMetadatas = new PropertyKeyMetadata[allMetadataCount];
            allMetadatas[0] = DefaultValueMetadata.Static(defaultValue);
            allMetadatas[1] = DependencyPropertyKeyMetadata.Attached;
            Array.Copy(metadatas, 0, allMetadatas, allMetadataCount - metadatas.Length, metadatas.Length);

            return new PropertyKey<T>(name, ownerType, allMetadatas);
        }
    }
}
