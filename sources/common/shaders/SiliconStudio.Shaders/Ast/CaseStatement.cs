// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A single case or default statement.
    /// </summary>
    public partial class CaseStatement : Statement
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "CaseStatement" /> class.
        /// </summary>
        public CaseStatement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CaseStatement"/> class.
        /// </summary>
        /// <param name="case">
        /// The @case.
        /// </param>
        public CaseStatement(Expression @case)
        {
            Case = @case;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the case.
        /// </summary>
        /// <value>
        ///   The case.
        /// </value>
        /// <remarks>
        ///   If this property is null, this is a default statement.
        /// </remarks>
        public Expression Case { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            if (Case != null)
            {
                ChildrenList.Add(Case);
            }

            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Case == null ? "default:" : string.Format("case {0}:", Case);
        }

        #endregion
    }
}
