// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A State type.
    /// </summary>
    public static class StateType
    {
        #region Constants and Fields

        /// <summary>
        /// A BlendState.
        /// </summary>
        public static readonly ObjectType BlendState = new ObjectType("BlendState");

        /// <summary>
        /// A DepthStencilState.
        /// </summary>
        public static readonly ObjectType DepthStencilState = new ObjectType("DepthStencilState");

        /// <summary>
        /// A RasterizerState
        /// </summary>
        public static readonly ObjectType RasterizerState = new ObjectType("RasterizerState");

        /// <summary>
        /// A SamplerState.
        /// </summary>
        public static readonly ObjectType SamplerState = new ObjectType("SamplerState");

        /// <summary>
        /// An old sampler_state declaration.
        /// </summary>
        public static readonly ObjectType SamplerStateOld = new ObjectType("sampler_state");

        /// <summary>
        /// A SamplerComparisonState.
        /// </summary>
        public static readonly ObjectType SamplerComparisonState = new ObjectType("SamplerComparisonState");

        private static readonly ObjectType[] ObjectTypes = new[] { BlendState, DepthStencilState, RasterizerState, SamplerState, SamplerStateOld, SamplerComparisonState };
        private static readonly ObjectType[] SamplerStateTypes = new[] { SamplerState, SamplerStateOld, SamplerComparisonState };

        #endregion

        public static bool IsStateType(this TypeBase type)
        {
            return type != null && Parse(type.Name) != null;
        }

        public static bool IsSamplerStateType(this TypeBase type)
        {
            if (type == null)
                return false;

            foreach (var objectType in SamplerStateTypes)
            {
                if (string.Compare(type.Name.Text, objectType.Name.Text, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Parses the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ObjectType Parse(string name)
        {
            foreach (var objectType in ObjectTypes)
            {
                if (string.Compare(name, objectType.Name.Text, StringComparison.OrdinalIgnoreCase) == 0)
                    return objectType;
            }
            return null;
        }
    }
}
