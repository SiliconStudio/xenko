// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A tag interface for an object referencing a type.
    /// </summary>
    public interface ITypeInferencer
    {
        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        /// <value>
        /// The reference.
        /// </value>
        TypeInference TypeInference { get; set; }
    }
}
