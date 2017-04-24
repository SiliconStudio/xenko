// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// Describes an attribute.
    /// </summary>
    public partial class AttributeDeclaration : AttributeBase
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "AttributeDeclaration" /> class.
        /// </summary>
        public AttributeDeclaration()
        {
            Parameters = new List<Literal>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value>
        ///   The name.
        /// </value>
        public Identifier Name { get; set; }

        /// <summary>
        ///   Gets or sets the parameters.
        /// </summary>
        /// <value>
        ///   The parameters.
        /// </value>
        public List<Literal> Parameters { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            return Parameters;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("[{0}({1})]", Name, string.Join(",", Parameters));
        }

        #endregion
    }
}
