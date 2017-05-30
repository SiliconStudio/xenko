// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
