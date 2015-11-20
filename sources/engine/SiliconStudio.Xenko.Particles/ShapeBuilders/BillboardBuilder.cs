// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("BillboardBuilder")]
    [Display("Billboard")]
    public class BillboardBuilder : ShapeBuilderBase
    {
        public override unsafe int BuildVertexBuffer(MappedResource vertexBuffer, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity, ParticlePool pool)
        {
            var numberOfParticles = Math.Min(remainingCapacity / 4, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return 0;

            var positionField = pool.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            
            var colorField = pool.GetField(ParticleFields.Color);
            var sizeField = pool.GetField(ParticleFields.Size);

            var vertices = (ParticleVertex*)vertexBuffer.DataBox.DataPointer;


            var renderedParticles = 0;

            foreach (var particle in pool)
            {
                // TODO Sorting

                var vertex = new ParticleVertex();
                vertex.Color = colorField.IsValid() ? (uint)particle.Get(colorField).ToRgba() : 0xFFFFFFFF;

                var centralPos = particle.Get(positionField); // TODO Local vs World emitters

                var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;
                var unitX = invViewX * particleSize; // TODO Rotation
                var unitY = invViewY * particleSize; // TODO Rotation

                vertex.Size = particleSize;

                // 0f 0f
                vertex.Position = centralPos - unitX - unitY;
                vertex.TexCoord = new Vector2(0, 0);
                *vertices++ = vertex;

                // 0f 1f
                vertex.Position = centralPos - unitX + unitY;
                vertex.TexCoord = new Vector2(0, 1);
                *vertices++ = vertex;

                // 1f 1f
                vertex.Position = centralPos + unitX + unitY;
                vertex.TexCoord = new Vector2(1, 1);
                *vertices++ = vertex;

                // 1f 0f
                vertex.Position = centralPos + unitX - unitY;
                vertex.TexCoord = new Vector2(1, 0);
                *vertices++ = vertex;

                remainingCapacity -= 4;

                if (++renderedParticles >= numberOfParticles)
                    return renderedParticles;
            }

            return renderedParticles;
        }
    }
}
