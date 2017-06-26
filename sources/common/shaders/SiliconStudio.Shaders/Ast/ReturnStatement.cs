// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A return statement.
    /// </summary>
    public partial class ReturnStatement : Statement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnStatement"/> class.
        /// </summary>
        public ReturnStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnStatement"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ReturnStatement(Expression value)
        {
            Value = value;
        }

        #region Public Properties

        /// <summary>
        ///   Gets or sets the value.
        /// </summary>
        /// <value>
        ///   The value.
        /// </value>
        /// <remarks>
        ///   If this value is null, return without any expression.
        /// </remarks>
        public Expression Value { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Value);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("return{0};", Value != null ? " " + Value : string.Empty);
        }

        #endregion
    }
}
