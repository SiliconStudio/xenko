// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Particles.ShapeBuilders.Tools;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    public enum TexCoordsPolicy
    {
        AsIs,
        Stretched,
        DistanceBased,        
    }

    /// <summary>
    /// Shape builder which builds each particle as a camera-facing quad
    /// </summary>
    [DataContract("ShapeBuilderRibbon")]
    [Display("Ribbon")]
    public class ShapeBuilderRibbon : ShapeBuilder
    {
        private readonly Ribbonizer ribbonizer = new Ribbonizer();

        /// <summary>
        /// Specifies how texture coordinates for the ribbons should be built
        /// </summary>
        /// <userdoc>
        /// Specifies how texture coordinates for the ribbons should be built
        /// </userdoc>
        [DataMember(10)]
        [Display("UV Coords")]
        public TexCoordsPolicy TexCoordsPolicy { get; set; } = TexCoordsPolicy.AsIs;

        /// <summary>
        /// The factor (coefficient) for length to use when building texture coordinates
        /// </summary>
        /// <userdoc>
        /// The factor (coefficient) for length to use when building texture coordinates
        /// </userdoc>
        [DataMember(20)]
        [Display("UV Factor")]
        public float TexCoordsFactor { get; set; } = 1f;

        [DataMember(30)]
        [Display("UV Rotate")]
        public UVRotate UvRotate { get; set; }

        /// <inheritdoc />
        public override int QuadsPerParticle { get; protected set; } = 1;

        /// <inheritdoc />
        public override void SetRequiredQuads(int quadsPerParticle, int livingParticles, int totalParticles)
        {
            ribbonizer.Restart(totalParticles);            
        }

        /// <inheritdoc />
        public override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Get all the required particle fields
            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            var sizeField = sorter.GetField(ParticleFields.Size);

            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(new Quaternion(0, 0, 0, 1)));


            var renderedParticles = 0;

            foreach (var particle in sorter)
            {
                var centralPos = particle.Get(positionField);

                var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;

                if (!trsIdentity)
                {
                    spaceRotation.Rotate(ref centralPos);
                    centralPos = centralPos * spaceScale + spaceTranslation;
                    particleSize *= spaceScale;
                }
                
                ribbonizer.AddParticle(ref centralPos, particleSize);

                renderedParticles++;
            }

            ribbonizer.Ribbonize(vtxBuilder, invViewX, invViewY, QuadsPerParticle, TexCoordsPolicy, TexCoordsFactor, UvRotate);

            var vtxPerShape = 4 * QuadsPerParticle;
            return renderedParticles * vtxPerShape;
        }

        sealed class Ribbonizer
        {
            private int capacity = 1;
            private int lastParticle = 0;
            private Vector3[] positions = new Vector3[1];
            private float[] sizes = new float[1];


            public void Restart(int newCapacity)
            {
                lastParticle = 0;

                if (newCapacity > positions.Length)
                {
                    positions = new Vector3[newCapacity];
                    sizes = new float[newCapacity];
                }                
            }

            public void AddParticle(ref Vector3 position, float size)
            {
                positions[lastParticle] = position;
                sizes[lastParticle] = size;
                lastParticle++;
            }

            private Vector3 GetWidthVector(float particleSize, ref Vector3 invViewX, ref Vector3 invViewY, ref Vector3 invViewZ, ref Vector3 axis0, ref Vector3 axis1)
            {
                // Simplest
                // return invViewX * (particleSize * 0.5f);

                // Camera-oriented
                var unitX = axis0 + axis1;
                var rotationQuaternion = Quaternion.RotationAxis(invViewZ, -MathUtil.PiOverTwo);
                rotationQuaternion.Rotate(ref unitX);
                unitX.Normalize();

                return unitX * (particleSize * 0.5f);
            }

            public unsafe void Ribbonize(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, int quadsPerParticle, TexCoordsPolicy texPolicy, float texFactor, UVRotate uvRotate)
            {
                if (lastParticle <= 0)
                    return;

                var posAttribute = vtxBuilder.GetAccessor(VertexAttributes.Position);
                var texAttribute = vtxBuilder.GetAccessor(vtxBuilder.DefaultTexCoords);

                if (lastParticle <= 1)
                {
                    // Optional - connect first particle to the origin/emitter

                    // Draw a dummy quad for the first particle
                    var particlePos = new Vector3(0, 0, 0);
                    var uvCoord = new Vector2(0, 0);

                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();

                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();

                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();

                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();
                    return;
                }

                vtxBuilder.SetVerticesPerSegment(quadsPerParticle * 6, quadsPerParticle * 4, quadsPerParticle * 2);

                // Step 1 - Determine the origin of the ribbon
                var invViewZ = Vector3.Cross(invViewX, invViewY);
                invViewZ.Normalize();

                var axis0 = positions[0] - positions[1];
                axis0.Normalize();

                var oldPoint = positions[0];
                var oldUnitX = GetWidthVector(sizes[0], ref invViewX, ref invViewY, ref invViewZ, ref axis0, ref axis0);

                // Step 2 - Draw each particle, connecting it to the previous (front) position

                var vCoordOld = 0f;

                for (int i = 0; i < lastParticle; i++)
                {
                    var centralPos = positions[i];

                    var particleSize = sizes[i];

                    // Directions for smoothing
                    var axis1 = (i + 1 < lastParticle) ? positions[i] - positions[i + 1] : positions[lastParticle - 2] - positions[lastParticle - 1];
                    axis1.Normalize();

                    var unitX = GetWidthVector(particleSize, ref invViewX, ref invViewY, ref invViewZ, ref axis0, ref axis1);

                    axis0 = axis1;

                    // Particle rotation - intentionally IGNORED for ribbon

                    var particlePos = oldPoint - oldUnitX;
                    var uvCoord = new Vector2(0, 0);
                    var rotatedCoord = uvCoord;


                    // Top Left - 0f 0f
                    uvCoord.Y = (texPolicy == TexCoordsPolicy.AsIs) ? 0 : vCoordOld;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    rotatedCoord = uvRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Top Right - 1f 0f
                    particlePos += oldUnitX * 2;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    uvCoord.X = 1;
                    rotatedCoord = uvRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Move the position to the next particle in the ribbon
                    particlePos += centralPos - oldPoint;
                    particlePos += unitX - oldUnitX;
                    vCoordOld = (texPolicy == TexCoordsPolicy.Stretched) ? 
                        ((i + 1)/(float)(lastParticle) * texFactor) : ((centralPos - oldPoint).Length() * texFactor) + vCoordOld;


                    // Bottom Left - 1f 1f
                    uvCoord.Y = (texPolicy == TexCoordsPolicy.AsIs) ? 1 : vCoordOld;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    rotatedCoord = uvRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Bottom Right - 0f 1f
                    particlePos -= unitX * 2;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));

                    uvCoord.X = 0;
                    rotatedCoord = uvRotate.GetCoords(uvCoord);
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&rotatedCoord));

                    vtxBuilder.NextVertex();


                    // Preserve the old attributes for the next cycle
                    oldUnitX = unitX;
                    oldPoint = centralPos;
                }
            }
        }
    }
}
