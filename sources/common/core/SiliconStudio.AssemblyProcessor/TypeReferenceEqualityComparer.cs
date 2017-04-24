// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using Mono.Cecil;

namespace SiliconStudio.AssemblyProcessor
{
    /// <summary>
    /// EqualityComparer for TypeReference, using FullName to compare.
    /// </summary>
    public class TypeReferenceEqualityComparer : EqualityComparer<TypeReference>
    {
        public new static readonly TypeReferenceEqualityComparer Default = new TypeReferenceEqualityComparer();

        public override bool Equals(TypeReference x, TypeReference y)
        {
            return x.FullName == y.FullName;
        }

        public override int GetHashCode(TypeReference obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
