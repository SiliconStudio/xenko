// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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