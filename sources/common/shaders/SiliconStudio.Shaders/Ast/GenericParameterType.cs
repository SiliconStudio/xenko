// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Defines a generic parameter type.
    /// </summary>
    public partial class GenericParameterType : TypeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterType"/> class.
        /// </summary>
        public GenericParameterType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GenericParameterType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public GenericParameterType(Identifier name)
            : base(name)
        {
        }
    }
}
