// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A raw asm expression.
    /// </summary>
    public partial class AsmExpression : Expression
    {
        #region Public Properties

        /// <summary>
        ///   Gets or sets the asm expression in raw text form.
        /// </summary>
        /// <value>
        ///   The asm expression in raw text form.
        /// </value>
        public string Text { get; set; }

        #endregion

        public override string ToString()
        {
            return "asm { ... }";
        }
    }
}