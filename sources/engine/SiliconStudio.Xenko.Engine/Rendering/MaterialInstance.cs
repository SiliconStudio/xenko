// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A <see cref="Material"/> instance.
    /// </summary>
    [DataContract("MaterialInstance")]
    [InlineProperty]
    public class MaterialInstance : IEquatable<MaterialInstance>
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
        }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        /// <userdoc>The reference to the material asset to used for this material slot name.</userdoc>
        [DataMember(10)]
        [InlineProperty]
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets if this instance is casting shadows.
        /// </summary>
        /// <value>A boolean indicating whether this instance is casting shadows. Default is <c>true</c>.</value>
        /// <userdoc>If checked, the model generates a shadow when enabling shadow maps.</userdoc>
        [DataMember(20)]
        [Display("Cast Shadows?")]
        [DefaultValue(true)]
        public bool IsShadowCaster { get; set; }

        /// <summary>
        /// Performs an explicit conversion from <see cref="Material"/> to <see cref="MaterialInstance"/>.
        /// </summary>
        /// <param name="material">The material.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator MaterialInstance(Material material)
        {
            return material == null ? null : new MaterialInstance(material);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var instance = (MaterialInstance)obj;
            return Material == instance.Material && IsShadowCaster == instance.IsShadowCaster;
        }

        public bool Equals(MaterialInstance other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Equals(Material, other.Material) && IsShadowCaster == other.IsShadowCaster;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Material?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ IsShadowCaster.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MaterialInstance left, MaterialInstance right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MaterialInstance left, MaterialInstance right)
        {
            return !Equals(left, right);
        }
    }
}
