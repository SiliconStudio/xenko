// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Shape builder which builds each particle as a up-facing quad, further rotated by the particle's rotation in 3d space
    /// </summary>
    [DataContract("ShapeBuilderQuad")]
    [Display("Quad")]
    public class ShapeBuilderQuad : ShapeBuilder
    {
        /// <inheritdoc />
        public override int QuadsPerParticle { get; protected set; } = 1;

        /// <inheritdoc />
        public unsafe override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, 
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Get all required particle fields
            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            var sizeField = sorter.GetField(ParticleFields.Size);
            var rotField = sorter.GetField(ParticleFields.Quaternion);
            var hasRotation = rotField.IsValid();


            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(new Quaternion(0, 0, 0, 1)));


            var renderedParticles = 0;

            var posAttribute = vtxBuilder.GetAccessor(VertexAttributes.Position);
            var texAttribute = vtxBuilder.GetAccessor(vtxBuilder.DefaultTexCoords);

            foreach (var particle in sorter)
            {
                var centralPos = particle.Get(positionField);

                var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;

                var unitX = new Vector3(1, 0, 0); 
                var unitY = new Vector3(0, 0, 1); 

                if (hasRotation)
                {
                    var particleRotation = particle.Get(rotField);
                    particleRotation.Rotate(ref unitX);
                    particleRotation.Rotate(ref unitY);
                }

                // The TRS matrix is not an identity, so we need to transform the quad
                if (!trsIdentity)
                {
                    spaceRotation.Rotate(ref centralPos);
                    centralPos = centralPos * spaceScale + spaceTranslation;
                    particleSize *= spaceScale;

                    spaceRotation.Rotate(ref unitX);
                    spaceRotation.Rotate(ref unitY);
                }

                // Use half size to make a Size = 1 result in a Billboard of 1m x 1m
                unitX *= (particleSize * 0.5f);
                unitY *= (particleSize * 0.5f);

                var particlePos = centralPos - unitX + unitY;
                var uvCoord = new Vector2(0, 0);
                // 0f 0f
                vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                vtxBuilder.NextVertex();


                // 1f 0f
                particlePos += unitX * 2;
                uvCoord.X = 1;
                vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                vtxBuilder.NextVertex();


                // 1f 1f
                particlePos -= unitY * 2;
                uvCoord.Y = 1;
                vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                vtxBuilder.NextVertex();


                // 0f 1f
                particlePos -= unitX * 2;
                uvCoord.X = 0;
                vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                vtxBuilder.NextVertex();

                renderedParticles++;
            }

            var vtxPerShape = 4 * QuadsPerParticle;
            return renderedParticles * vtxPerShape;
        }
    }
}
