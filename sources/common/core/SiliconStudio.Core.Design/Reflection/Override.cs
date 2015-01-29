// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// This class is holding the PropertyKey using to store <see cref="OverrideType"/> per object into the <see cref="ShadowObject"/>.
    /// </summary>
    public static class Override
    {
        /// <summary>
        /// The OverrideType key.
        /// </summary>
        public static readonly PropertyKey<OverrideType> OverrideKey = new PropertyKey<OverrideType>("Override", typeof(Override), DefaultValueMetadata.Static(OverrideType.New));

        /// <summary>
        /// Gets the override for the specified member.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="memberDescriptor">The member descriptor.</param>
        /// <returns>OverrideType.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberDescriptor
        /// </exception>
        public static OverrideType GetOverride(this object instance, IMemberDescriptor memberDescriptor)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (memberDescriptor == null) throw new ArgumentNullException("memberDescriptor");
            OverrideType overrideType;
            return instance.TryGetDynamicProperty(memberDescriptor, OverrideKey, out overrideType) ? overrideType : OverrideType.Base;
        }

        /// <summary>
        /// Sets the override for the specified member.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="memberDescriptor">The member descriptor.</param>
        /// <param name="overrideType">Type of the override.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// memberDescriptor
        /// </exception>
        public static void SetOverride(this object instance, IMemberDescriptor memberDescriptor, OverrideType overrideType)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (memberDescriptor == null) throw new ArgumentNullException("memberDescriptor");
            instance.SetDynamicProperty(memberDescriptor, OverrideKey, overrideType);
        }
    }
}