// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// A Empty of statement.
    /// </summary>
    public partial class EmptyStatement : Statement
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EmptyStatement" /> class.
        /// </summary>
        public EmptyStatement()
        {
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Empty;
        }
        #endregion
    }
}
