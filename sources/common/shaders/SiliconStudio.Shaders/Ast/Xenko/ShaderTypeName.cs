// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// A typeless reference.
    /// </summary>
    public partial class ShaderTypeName : TypeName
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderTypeName"/> class.
        /// </summary>
        public ShaderTypeName()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderTypeName"/> class.
        /// </summary>
        /// <param name="typeBase">The type base.</param>
        public ShaderTypeName(TypeBase typeBase)
            : base(typeBase.Name)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderTypeName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ShaderTypeName(Identifier name) : base(name)
        {
        }
    }
}