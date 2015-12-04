// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("ShapeBuilderHexagon")]
    [Display("Hexagon")]
    public class ShapeBuilderHexagon : ShapeBuilderBase
    {
        public override int QuadsPerParticle { get; protected set; } = 2;

        public override unsafe int BuildVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticlePool pool)
        {
            var vtxPerShape = 4 * QuadsPerParticle;

            var numberOfParticles = Math.Min(remainingCapacity / vtxPerShape, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return 0;

            var positionField = pool.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;

            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(new Quaternion(0, 0, 0, 1)));

            var sizeField = pool.GetField(ParticleFields.Size);

            var renderedParticles = 0;

            // TODO Sorting

            foreach (var particle in pool)
            {
                var centralPos = particle.Get(positionField);

                var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;

                if (!trsIdentity)
                {
                    spaceRotation.Rotate(ref centralPos);
                    centralPos = centralPos * spaceScale + spaceTranslation;
                    particleSize *= spaceScale;
                    // TODO Rotation
                }

                var unitX = invViewX * particleSize; // TODO Rotation
                var unitY = invViewY * particleSize; // TODO Rotation

                // vertex.Size = particleSize;

                const float Sqrt3Half = 0.86602540378f;
                unitY *= Sqrt3Half;
                var halfX = unitX * 0.5f;

                var particlePos = centralPos - halfX + unitY;
                var uvCoord = new Vector2(0.25f, 0.5f - Sqrt3Half * 0.5f);


                // Upper half

                // 0f 0f
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 1f 0f
                particlePos += unitX;
                uvCoord.X = 0.75f;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 1f 1f
                particlePos += halfX;
                particlePos -= unitY;
                uvCoord.X = 1;
                uvCoord.Y = 0.5f;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 0f 1f
                particlePos -= unitX * 2;
                uvCoord.X = 0;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();

                // Upper half

                // 0f 0f
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 1f 0f
                particlePos += unitX * 2;
                uvCoord.X = 1;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 1f 1f
                particlePos -= halfX;
                particlePos -= unitY;
                uvCoord.X = 0.75f;
                uvCoord.Y = 1;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 0f 1f
                particlePos -= unitX;
                uvCoord.X = 0.25f;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();

                remainingCapacity -= vtxPerShape;

                if (++renderedParticles >= numberOfParticles)
                {
                    return renderedParticles;
                }
            }

            return renderedParticles;
        }
    }
}
