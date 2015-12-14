// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// This class is holding the PropertyKey using to store an Id per object into the <see cref="ShadowObject"/>.
    /// </summary>
    internal static class ShadowId
    {
        /// <summary>
        /// The OverrideType key.
        /// </summary>
        public static readonly PropertyKey<Guid> IdKey = new PropertyKey<Guid>("ID", typeof(ShadowId), DefaultValueMetadata.Static(Guid.Empty));

        /// <summary>
        /// Gets the ID for the specified member.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="id">The ID of the object</param>
        /// <returns>OverrideType.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberDescriptor
        /// </exception>
        public static bool GetId(this object instance, out Guid id)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            return instance.TryGetDynamicProperty(ThisDescriptor.Default, IdKey, out id);
        }

        /// <summary>
        /// Sets the override for the specified member.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="id">The ID</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberDescriptor
        /// </exception>
        public static void SetId(this object instance, Guid id)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            instance.SetDynamicProperty(ThisDescriptor.Default, IdKey, id);
        }
    }
}