// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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