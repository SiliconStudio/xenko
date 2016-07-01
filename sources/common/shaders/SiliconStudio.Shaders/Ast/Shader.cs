// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Toplevel container of a shader parsing result.
    /// </summary>
    public partial class Shader : Node
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "Shader" /> class.
        /// </summary>
        public Shader()
        {
            Declarations = new List<Node>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets the declarations.
        /// </summary>
        /// <value>
        ///   The declarations.
        /// </value>
        public List<Node> Declarations { get; set; }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override IEnumerable<Node> Childrens()
        {
            return Declarations;
        }

        #endregion
    }
}