// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// Shader Class that supports adding mixin class to its base classes.
    /// </summary>
    public partial class ShaderRootClassType : ShaderClassType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderRootClassType"/> class.
        /// </summary>
        public ShaderRootClassType()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderRootClassType"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ShaderRootClassType(string name)
            : base(name)
        {
        }
    }
}