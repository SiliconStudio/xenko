// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// C# namespace or class.
    /// </summary>
    public partial class IdentifierDot : CompositeIdentifier
    {
        /// <inheritdoc/>
        public override string Separator
        {
            get
            {
                return ".";
            }
        }
    }
}