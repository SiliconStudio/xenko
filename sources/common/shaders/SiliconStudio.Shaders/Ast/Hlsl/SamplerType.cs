// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A State type.
    /// </summary>
    public partial class SamplerType : ObjectType
    {
        #region Constants and Fields

        /// <summary>
        /// A sampler.
        /// </summary>
        public static readonly SamplerType Sampler = new SamplerType("sampler");

        /// <summary>
        /// A sampler1D.
        /// </summary>
        public static readonly SamplerType Sampler1D = new SamplerType("sampler1D");

        /// <summary>
        /// A sampler2D
        /// </summary>
        public static readonly SamplerType Sampler2D = new SamplerType("sampler2D");

        /// <summary>
        /// A sampler3D.
        /// </summary>
        public static readonly SamplerType Sampler3D = new SamplerType("sampler3D");

        /// <summary>
        /// A samplerCUBE.
        /// </summary>
        public static readonly SamplerType SamplerCube = new SamplerType("samplerCUBE");


        private static readonly SamplerType[] SamplerTypes = new[] { Sampler, Sampler1D, Sampler2D, Sampler3D, SamplerCube };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerType"/> class.
        /// </summary>
        public SamplerType()
        {
            IsBuiltIn = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="altNames">The alt names.</param>
        public SamplerType(string name, params string[] altNames)
            : base(name, altNames)
        {
            IsBuiltIn = true;
        }

        /// <inheritdoc/>
        public bool Equals(SamplerType other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Equals(obj as SamplerType);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(SamplerType left, SamplerType right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(SamplerType left, SamplerType right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static SamplerType Parse(string name)
        {
            return SamplerTypes.FirstOrDefault(stateType => string.Compare(name, stateType.Name.Text, true) == 0);
        }
    }
}