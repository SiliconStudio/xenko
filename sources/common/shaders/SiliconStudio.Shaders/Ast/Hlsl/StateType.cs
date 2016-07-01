// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A State type.
    /// </summary>
    public partial class StateType : ObjectType
    {
        #region Constants and Fields

        /// <summary>
        /// A BlendState.
        /// </summary>
        public static readonly StateType BlendState = new StateType("BlendState");

        /// <summary>
        /// A DepthStencilState.
        /// </summary>
        public static readonly StateType DepthStencilState = new StateType("DepthStencilState");

        /// <summary>
        /// A RasterizerState
        /// </summary>
        public static readonly StateType RasterizerState = new StateType("RasterizerState");

        private static readonly StateType[] StateTypes = new[] { BlendState, DepthStencilState, RasterizerState };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="StateType"/> class.
        /// </summary>
        public StateType()
        {
            IsBuiltIn = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateType"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public StateType(string name)
            : base(name)
        {
            IsBuiltIn = true;
        }

        /// <inheritdoc/>
        public bool Equals(StateType other)
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
            return Equals(obj as StateType);
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
        public static bool operator ==(StateType left, StateType right)
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
        public static bool operator !=(StateType left, StateType right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static StateType Parse(string name)
        {
            return StateTypes.FirstOrDefault(stateType => string.Compare(name, stateType.Name.Text, true) == 0);
        }
    }
}