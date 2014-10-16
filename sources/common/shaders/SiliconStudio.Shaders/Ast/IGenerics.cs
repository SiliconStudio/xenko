// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// An interface used by generic definitions and instance.
    /// </summary>
    public interface IGenerics
    {
        /// <summary>
        /// Gets or sets the generic arguments.
        /// </summary>
        /// <value>
        /// The generic arguments.
        /// </value>
        List<TypeBase> GenericParameters { get; set; }

        /// <inheritdoc/>
        List<TypeBase> GenericArguments { get; set; }
    }
}