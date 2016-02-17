// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    /// <summary>
    /// A list of common <see cref="AttributeDescription"/> used to access the vertex fileds in a <see cref="ParticleVertexBuilder"/>
    /// </summary>
    public static class VertexAttributes
    {
        public static AttributeDescription Position = new AttributeDescription("POSITION");

        public static AttributeDescription Color = new AttributeDescription("COLOR");

        public static AttributeDescription Lifetime = new AttributeDescription("BATCH_LIFETIME");

        public static AttributeDescription RandomSeed = new AttributeDescription("BATCH_RANDOMSEED");
    }

}
