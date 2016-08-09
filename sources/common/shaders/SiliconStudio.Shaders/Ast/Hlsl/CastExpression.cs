// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A cast expression.
    /// </summary>
    public partial class CastExpression : Expression
    {
        #region Public Properties

        /// <summary>
        ///   Gets or sets from.
        /// </summary>
        /// <value>
        ///   From.
        /// </value>
        public Expression From { get; set; }

        /// <summary>
        ///   Gets or sets the target.
        /// </summary>
        /// <value>
        ///   The target.
        /// </value>
        public TypeBase Target { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Target);
            ChildrenList.Add(From);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("({0}){1}", Target, From);
        }

        #endregion
    }
}