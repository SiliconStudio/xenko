// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Base root class for all statements.
    /// </summary>
    public abstract partial class Statement : Node, IAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class.
        /// </summary>
        protected Statement()
        {
            Attributes = new List<AttributeBase>();
        }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public List<AttributeBase> Attributes { get; set; }
    }
}
