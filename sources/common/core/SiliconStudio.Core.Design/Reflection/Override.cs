// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// This class is holding the PropertyKey using to store <see cref="OverrideType"/> per object into the <see cref="ShadowObject"/>.
    /// </summary>
    public static partial class Override
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
            if (memberDescriptor == null) throw new ArgumentNullException(nameof(memberDescriptor));
            OverrideType overrideType;
            return instance == null ? OverrideType.Base : instance.TryGetDynamicProperty(memberDescriptor, OverrideKey, out overrideType) ? overrideType : OverrideType.Base;
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
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (memberDescriptor == null) throw new ArgumentNullException(nameof(memberDescriptor));
            instance.SetDynamicProperty(memberDescriptor, OverrideKey, overrideType);
        }

        /// <summary>
        /// Remove all overrides information attached to an instance (Note that this method is not recursive and must be applied on all object).
        /// </summary>
        /// <param name="instance">An object instance</param>
        public static void RemoveFrom(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var shadow = ShadowObject.Get(instance);
            if (shadow == null)
            {
                return;
            }

            // Remove override information from an object
            foreach (var memberKey in shadow.Keys.ToList())
            {
                if (memberKey.Item2 == OverrideKey)
                {
                    shadow.Remove(memberKey);
                }
            }
        }
    }
}