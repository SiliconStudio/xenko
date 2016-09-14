// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// A params block.
    /// </summary>
    public partial class ParametersBlock : Node, IScopeContainer
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "ParametersBlock" /> class.
        /// </summary>
        public ParametersBlock()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametersBlock" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="statements">The statements.</param>
        public ParametersBlock(Identifier name, BlockStatement statements)
        {
            Name = name;
            Body = statements;
        }

        #endregion

        #region Public Properties

        public Identifier Name { get; set; }

        public BlockStatement Body { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Name);
            ChildrenList.Add(Body);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("params {0} {{...}}", Name);
        }

        #endregion
    }
}