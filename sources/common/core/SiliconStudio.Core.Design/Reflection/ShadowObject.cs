// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Allows to attach dynamic properties to an object at runtime.
    /// </summary>
    public static class ShadowObject
    {
        // Use a conditional weak table in order to attach properties and to 
        private static readonly ConditionalWeakTable<object, ShadowContainer> Shadows = new ConditionalWeakTable<object, ShadowContainer>();

        /// <summary>
        /// Gets the <see cref="ShadowContainer"/> instance.
        /// </summary>
        /// <param name="instance">The live instance.</param>
        /// <returns>The shadow instance or <c>null</c> if none</returns>
        internal static ShadowContainer GetShadow(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            ShadowContainer shadow;
            Shadows.TryGetValue(instance, out shadow);
            return shadow;
        }

        /// <summary>
        /// Copies all dynamic properties from an instance to another instance.
        /// </summary>
        /// <param name="fromInstance">The instance to copy the shadow attributes from</param>
        /// <param name="toInstance">The instance to copy the shadow attributes to</param>
        public static void CopyDynamicProperties(object fromInstance, object toInstance)
        {
            if (fromInstance == null) throw new ArgumentNullException(nameof(fromInstance));
            if (toInstance == null) throw new ArgumentNullException(nameof(toInstance));

            ShadowContainer shadow;
            if (Shadows.TryGetValue(fromInstance, out shadow))
            {
                var shadowClone = shadow.Clone();
                Shadows.Add(toInstance, shadowClone);
            }
        }

        /// <summary>
        /// Tries to get the value of a dynamic property.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="memberKey">The member key.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value attached.</param>
        /// <returns><c>true</c> if there is a value attached, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberKey
        /// or
        /// attributeKey
        /// </exception>
        public static bool TryGetDynamicProperty<T>(this object instance, object memberKey, PropertyKey<T> attributeKey, out T value)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (attributeKey == null) throw new ArgumentNullException("attributeKey");

            ShadowContainer shadow;
            ShadowAttributes attributes;
            value = default(T);
            return (Shadows.TryGetValue(instance, out shadow) && shadow.TryGetAttributes(memberKey, out attributes) && attributes.TryGetAttribute(attributeKey, out value));
        }

        /// <summary>
        /// Sets a dynamic property.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="instance">The instance object.</param>
        /// <param name="memberKey">The member key.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberKey
        /// or
        /// attributeKey
        /// </exception>
        public static void SetDynamicProperty<T>(this object instance, object memberKey, PropertyKey<T> attributeKey, T value)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (memberKey == null) throw new ArgumentNullException("memberKey");
            if (attributeKey == null) throw new ArgumentNullException("attributeKey");
            Shadows.GetOrCreateValue(instance)[memberKey].SetAttribute(attributeKey, value);
        }
    }
}