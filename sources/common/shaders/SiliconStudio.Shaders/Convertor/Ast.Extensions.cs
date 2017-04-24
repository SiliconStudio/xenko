// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Shaders.Ast;
using SiliconStudio.Shaders.Ast.Hlsl;

namespace SiliconStudio.Shaders.Convertor
{

    internal static class AstExtensions
    {
        public static Semantic Semantic(this Variable declarator)
        {
            return declarator.Qualifiers.OfType<Semantic>().LastOrDefault();
        }

        public static Semantic Semantic(this MethodDeclaration methodDeclaration)
        {
            return methodDeclaration.Qualifiers.OfType<Semantic>().LastOrDefault();
        }

        public static IEnumerable<AttributeDeclaration> Attributes(this MethodDeclaration methodDeclaration)
        {
            return methodDeclaration.Attributes.OfType<AttributeDeclaration>();
        }

        public static IEnumerable<Variable> Fields(this StructType structType)
        {
            return structType.Fields.SelectMany(variable => variable.Instances());
        }
    }
}
