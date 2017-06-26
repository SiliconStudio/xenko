// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class MemberName : TypeBase, IDeclaration, IScopeContainer, IGenericStringArgument
    {
        #region Constructors and Destructors
        /// <summary>
        ///   Initializes a new instance of the <see cref = "MemberName" /> class.
        /// </summary>
        public MemberName()
            : base("MemberName")
        {
        }

        #endregion
    }
}
