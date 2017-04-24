// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Toplevel interface for a declaration.
    /// </summary>
    public interface IDeclaration
    {
        /// <summary>
        ///   Gets or sets the name of this declaration
        /// </summary>
        /// <value>
        ///   The name.
        /// </value>
        Identifier Name { get; set; }
    }
}
