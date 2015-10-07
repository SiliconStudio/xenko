// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Diff
{
    /// <summary>
    /// Attribute used to improve the diff behaviour for a specific field/property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DiffMemberAttribute : Attribute
    {
        /// <summary>
        /// If not null, this will be used ot prefer one merge source.
        /// </summary>
        public Diff3ChangeType? PreferredChange { get; set; }

        /// <summary>
        /// The weight to use when calculating the final matching weight. By default 0.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public DiffMemberAttribute()
        {
        }

        /// <summary>
        /// Initializes this instance with the specified preferred changed.
        /// </summary>
        /// <param name="preferredChange">A preferred change.</param>
        public DiffMemberAttribute(Diff3ChangeType preferredChange)
        {
            PreferredChange = preferredChange;
        }
    }
}