// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// Attribute to define for a <see cref="IAssetCompiler"/> for a <see cref="Asset"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class CompilerAttribute : DynamicTypeAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerAttribute"/> class.
        /// </summary>
        /// <param name="type">The type must be of type <see cref="IAssetCompiler"/>.</param>
        protected CompilerAttribute(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerAttribute"/> class.
        /// </summary>
        /// <param name="typeName">The type must be of type <see cref="IAssetCompiler"/>.</param>
        protected CompilerAttribute(string typeName)
            : base(typeName)
        {
        }
    }
}