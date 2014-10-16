// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public class NamespaceBlock : TypeBase, IScopeContainer
    {
        #region Public Properties

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