// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    class FixupValueTypeVisitor : CecilTypeReferenceVisitor
    {
        public static readonly FixupValueTypeVisitor Default = new FixupValueTypeVisitor();

        public override TypeReference Visit(TypeReference type)
        {
            var typeDefinition = type.Resolve();
            if (typeDefinition.IsValueType && !type.IsValueType)
                type.IsValueType = typeDefinition.IsValueType;

            return base.Visit(type);
        }
    }
}
