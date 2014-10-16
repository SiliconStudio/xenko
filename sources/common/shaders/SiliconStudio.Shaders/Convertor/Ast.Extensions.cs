// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
