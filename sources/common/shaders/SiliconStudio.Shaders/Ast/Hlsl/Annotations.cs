// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Shaders.Visitor;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// An Annotations.
    /// </summary>
    public partial class Annotations : PostAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Annotations"/> class.
        /// </summary>
        public Annotations()
        {
            Variables = new List<Variable>();
        }

        /// <summary>
        /// Gets or sets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public List<Variable> Variables { get; set; }
    }
}
