// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class NamespaceBlock : TypeBase, IScopeContainer
    {
        #region Public Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceBlock"/> class.
        /// </summary>
        public NamespaceBlock() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeBase" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NamespaceBlock(string name)
            : base(name)
        {
            Body = new List<Node>();
        }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public List<Node> Body { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            ChildrenList.Clear();
            ChildrenList.Add(Name);
            ChildrenList.AddRange(Body);
            return ChildrenList;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("namespace {0} {{...}}", Name);
        }

        #endregion
    }
}
