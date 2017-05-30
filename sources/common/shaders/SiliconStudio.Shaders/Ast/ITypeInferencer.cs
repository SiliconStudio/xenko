// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
