// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A reference to a type.
    /// </summary>
    [DataContract]
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
