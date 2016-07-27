// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Expression used to initliaze an array {...expressions,}
    /// </summary>
    public partial class ArrayInitializerExpression : Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayInitializerExpression"/> class.
        /// </summary>
        public ArrayInitializerExpression()
        {
            Items = new List<Expression>();
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public List<Expression> Items { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<Node> Childrens()
        {
            return Items;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(",", Items));
        }
    }
}