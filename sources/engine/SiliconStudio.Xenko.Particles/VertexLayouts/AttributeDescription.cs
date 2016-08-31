// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
