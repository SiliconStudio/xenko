// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{

    /// <summary>
    /// A group of cases and default attached to their statements.
    /// </summary>
    public partial class SwitchCaseGroup : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchCaseGroup"/> class.
        /// </summary>
        public SwitchCaseGroup()
        {
            Cases = new List<CaseStatement>();
            Statements = new StatementList();
        }

        #region Public Properties

        /// <summary>
        ///   Gets or sets the cases.
        /// </summary>
        /// <value>
        ///   The cases.
        /// </value>
        public List<CaseStatement> Cases { get; set; }

        /// <summary>
        ///   Gets or sets the statements.
        /// </summary>
        /// <value>
        ///   The statements.
        /// </value>
        public StatementList Statements { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.AddRange(Cases);
            ChildrenList.Add(Statements);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("{0} ...", string.Join("\n", Cases));
        }

        #endregion
    }
}
