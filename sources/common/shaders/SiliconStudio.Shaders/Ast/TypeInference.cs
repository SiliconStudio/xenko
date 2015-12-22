// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A reference to a type.
    /// </summary>
    public class TypeInference
    {
        /// <summary>
        /// Gets or sets the declaration.
        /// </summary>
        /// <value>
        /// The declaration.
        /// </value>
        public IDeclaration Declaration { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public TypeBase TargetType { get; set; }

        /// <summary>
        /// Gets or sets the expected type.
        /// </summary>
        /// <value>
        /// The expected type.
        /// </value>
        public TypeBase ExpectedType { get; set; }

        /// <inheritdoc/>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
