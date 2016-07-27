// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    /// <summary>
    /// A structure.
    /// </summary>
    public partial class VarType : TypeBase, IDeclaration, IScopeContainer
    {
        #region Constructors and Destructors
        /// <summary>
        ///   Initializes a new instance of the <see cref = "StructType" /> class.
        /// </summary>
        public VarType() : base("var")
        {
        }

        #endregion
    }
}