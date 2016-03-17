// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Common interface for the description of a material.
    /// </summary>
    public interface IMaterialDescriptor : IMaterialShaderGenerator
    {
        /// <summary>
        /// Gets the material identifier used only internaly to match material instance by id (when cloning an asset for example)
        /// to provide an error when defining a material that is recursively referencing itself.
        /// </summary>
        /// <value>The material identifier.</value>
        [DataMemberIgnore]
        Guid MaterialId { get; }

        /// <summary>
        /// Gets or sets the material attributes.
        /// </summary>
        /// <value>The material attributes.</value>
        [DataMember(10)]
        [NotNull]
        [Display("Attributes", Expand = ExpandRule.Always)]
        MaterialAttributes Attributes { get; set; }

        /// <summary>
        /// Gets or sets the material compositor.
        /// </summary>
        /// <value>The material compositor.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        [NotNull]
        [NotNullItems]
        MaterialBlendLayers Layers { get; set; }
    }
}