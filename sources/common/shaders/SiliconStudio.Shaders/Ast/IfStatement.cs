// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// If statement.
    /// </summary>
    public partial class IfStatement : Statement
    {
        #region Public Properties

        /// <summary>
        ///   Gets or sets the condition.
        /// </summary>
        /// <value>
        ///   The condition.
        /// </value>
        public Expression Condition { get; set; }

        /// <summary>
        ///   Gets or sets the else.
        /// </summary>
        /// <value>
        ///   The else.
        /// </value>
        public Statement Else { get; set; }

        /// <summary>
        ///   Gets or sets the then.
        /// </summary>
        /// <value>
        ///   The then.
        /// </value>
        public Statement Then { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Condition);
            ChildrenList.Add(Then);
            ChildrenList.Add(Else);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("if ({0}) then {{...}}{1}", Condition, Else == null ? string.Empty : "...");
        }

        #endregion
    }
}