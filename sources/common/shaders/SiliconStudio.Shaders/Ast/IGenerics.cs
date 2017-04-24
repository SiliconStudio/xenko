// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
