// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// A <see cref="Material"/> instance.
    /// </summary>
    [DataContract("MaterialInstance")]
    [InlineProperty]
    public class MaterialInstance
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialInstance"/> class.
        /// </summary>
        public MaterialInstance() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialInstance"/> class.
        /// </summary>
        /// <param name="material">The material.</param>
        public MaterialInstance(Material material)
        {
            Material = material;
            IsShadowCaster = true;
            IsShadowReceiver = true;
        }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DataMember(10)]
        [InlineProperty]
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets if this instance is casting shadows.
        /// </summary>
        /// <value>A boolean indicating whether this instance is casting shadows. Default is <c>true</c>.</value>
        [DataMember(20)]
        [Display("Cast Shadows?")]
        [DefaultValue(true)]
        public bool IsShadowCaster { get; set; }

        /// <summary>
        /// Gets or sets if this instance is receiving shadows.
        /// </summary>
        /// <value>A boolean indicating whether this instance is receiving shadows. Default is <c>true</c>.</value>
        [DataMember(30)]
        [Display("Receive Shadows?")]
        [DefaultValue(true)]
        public bool IsShadowReceiver { get; set; }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Material"/> to <see cref="MaterialInstance"/>.
        /// </summary>
        /// <param name="material">The material.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator MaterialInstance(Material material)
        {
            return material == null ? null : new MaterialInstance(material);
        }
    }
}