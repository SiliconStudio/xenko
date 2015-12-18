// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public static class ParticleVertexElements
    {
        public static VertexElement Position = VertexElement.Position<Vector3>();

        public static VertexElement TexCoord = VertexElement.TextureCoordinate<Vector2>();

        public static VertexElement Color    = VertexElement.Color<Color>();

        public static VertexElement Lifetime = new VertexElement("BATCH_LIFETIME", PixelFormat.R32_Float);

        public static VertexElement RandSeed = new VertexElement("BATCH_RANDOMSEED", PixelFormat.R32_Float);

    }
}
