// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// This attribute indicates that the associated property should be inlined in its container presentation
    /// when displayed in a property grid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class InlinePropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets whether to expand the inline property in the UI. The default is <see cref="ExpandRule.Never"/>.
        /// </summary>
        public ExpandRule Expand { get; set; } = ExpandRule.Never;
    }
}
