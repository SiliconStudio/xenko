// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Ast.Xenko
{
    public partial class LiteralIdentifier : Identifier
    {
        public LiteralIdentifier()
        {
        }

        public LiteralIdentifier(Literal valueName)
            : base(valueName.ToString())
        {
            Value = valueName;
        }


        public Literal Value { get; set; }
    }
}