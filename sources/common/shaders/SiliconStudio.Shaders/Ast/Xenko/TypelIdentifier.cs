// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class TypeIdentifier : Identifier
    {
        public TypeIdentifier()
        {
        }

        public TypeIdentifier(TypeBase type)
            : base(type.ToString())
        {
            Type = type;
        }

        public TypeBase Type { get; set; }
    }
}