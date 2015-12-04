// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("ShapeBuilderBillboard")]
    [Display("Billboard")]
    public class ShapeBuilderBillboard : ShapeBuilderBase
    {
        public override unsafe int BuildVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, ref int remainingCapacity,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticlePool pool)
        {
            var numberOfParticles = Math.Min(remainingCapacity / 4, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return 0;

            var positionField = pool.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;

            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(new Quaternion(0, 0, 0, 1)));


            var colorField  = pool.GetField(ParticleFields.Color);
            var sizeField   = pool.GetField(ParticleFields.Size);

            var randField   = pool.GetField(ParticleFields.RandomSeed);
            var lifeField   = pool.GetField(ParticleFields.RemainingLife);

            var whiteColor = new Color4(1, 1, 1, 1);
            var renderedParticles = 0;

            // TODO Sorting

            foreach (var particle in pool)
            {
                // Some attributes only need to be set once for the entire particle
                vtxBuilder.SetColorForParticle(colorField.IsValid() ? particle[colorField] : (IntPtr)(&whiteColor));

                vtxBuilder.SetLifetimeForParticle(particle[lifeField]);

                vtxBuilder.SetRandomSeedForParticle(particle[randField]);



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

                var particlePos = centralPos - unitX + unitY;
                var uvCoord = new Vector2(0, 0);
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
                particlePos -= unitY * 2;
                uvCoord.Y = 1;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                // 0f 1f
                particlePos -= unitX * 2;
                uvCoord.X = 0;
                vtxBuilder.SetPosition(ref particlePos);
                vtxBuilder.SetUvCoords(ref uvCoord);
                vtxBuilder.NextVertex();


                remainingCapacity -= 4;

                if (++renderedParticles >= numberOfParticles)
                {
                    return renderedParticles;
                }
            }

            return renderedParticles;
        }
    }
}
