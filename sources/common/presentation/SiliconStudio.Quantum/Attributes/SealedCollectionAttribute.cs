// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Attributes
{
    public sealed class SealedCollectionAttribute : Attribute
    {
        public SealedCollectionAttribute()
            : this(true)
        {
        }

        public SealedCollectionAttribute(bool collectionSealed)
        {
            CollectionSealed = collectionSealed;
        }

        public bool CollectionSealed { get; set; }
    }
}
