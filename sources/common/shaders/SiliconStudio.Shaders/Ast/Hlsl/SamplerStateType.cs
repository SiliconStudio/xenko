// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A State type.
    /// </summary>
    public partial class SamplerStateType : ObjectType
    {
        #region Constants and Fields

        /// <summary>
        /// A SamplerState.
        /// </summary>
        public static readonly SamplerStateType SamplerState = new SamplerStateType("SamplerState");

        /// <summary>
        /// An old sampler_state declaration.
        /// </summary>
        public static readonly SamplerStateType SamplerStateOld = new SamplerStateType("sampler_state");

        /// <summary>
        /// A SamplerComparisonState.
        /// </summary>
        public static readonly SamplerStateType SamplerComparisonState = new SamplerStateType("SamplerComparisonState");


        private static readonly SamplerStateType[] SamplerStateTypes = new[] { SamplerState, SamplerStateOld, SamplerComparisonState };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateType"/> class.
        /// </summary>
        public SamplerStateType()
        {
            IsBuiltIn = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerStateType"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public SamplerStateType(string name)
            : base(name)
        {
            IsBuiltIn = true;
        }

        /// <inheritdoc/>
        public bool Equals(SamplerStateType other)
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
            return Equals(obj as SamplerStateType);
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
        public static bool operator ==(SamplerStateType left, SamplerStateType right)
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
        public static bool operator !=(SamplerStateType left, SamplerStateType right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static SamplerStateType Parse(string name)
        {
            return SamplerStateTypes.FirstOrDefault(stateType => string.Compare(name, stateType.Name.Text, true) == 0);
        }
    }
}