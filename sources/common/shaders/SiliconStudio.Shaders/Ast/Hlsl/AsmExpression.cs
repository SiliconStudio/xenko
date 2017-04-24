// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
