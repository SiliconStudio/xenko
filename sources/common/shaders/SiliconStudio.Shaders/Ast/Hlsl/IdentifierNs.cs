// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Ast.Hlsl
{
    /// <summary>
    /// A C++ identifier with namespaces "::" separator
    /// </summary>
    public partial class IdentifierNs : CompositeIdentifier
    {
        /// <inheritdoc/>
        public override string Separator
        {
            get
            {
                return "::";
            }
        }
    }
}