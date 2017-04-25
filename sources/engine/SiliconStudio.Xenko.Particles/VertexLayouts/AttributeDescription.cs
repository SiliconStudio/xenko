// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    /// <summary>
    /// Attribute description code used for defining vertex attributes in the <see cref="ParticleVertexBuilder"/>
    /// </summary>
    public class AttributeDescription : IEquatable<AttributeDescription>
    {
        private readonly int hashCode;
        private readonly string name;
        public string Name => name;

        public override int GetHashCode() => hashCode;

        public AttributeDescription(string name)
        {
            this.name = name;
            hashCode = name?.GetHashCode() ?? 0;
        }

        public bool Equals(AttributeDescription other) => (hashCode == other.hashCode);
    }
}
