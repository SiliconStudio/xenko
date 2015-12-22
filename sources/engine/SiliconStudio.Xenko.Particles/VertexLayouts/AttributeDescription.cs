// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public struct AttributeDescription
    {
        private readonly int hashCode;
        public override int GetHashCode() => hashCode;

        public AttributeDescription(string name)
        {
            hashCode = name?.GetHashCode() ?? 0;
        }
    }
}
