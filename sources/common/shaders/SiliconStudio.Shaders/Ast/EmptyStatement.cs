// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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