// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
