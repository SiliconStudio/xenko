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
    /// Shape builder which builds each particle as a camera-facing quad
    /// </summary>
    [DataContract("ShapeBuilderRibbon")]
    [Display("Ribbon")]
    public class ShapeBuilderRibbon : ShapeBuilder
    {
        private Ribbonizer ribbonizer = new Ribbonizer();

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

            ribbonizer.Ribbonize(vtxBuilder, invViewX, invViewY);

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

            public unsafe void Ribbonize(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY)
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

                // Step 1 - Determine the origin of the ribbon
                var invViewZ = Vector3.Cross(invViewX, invViewY);
                invViewZ.Normalize();

                var axis0 = positions[0] - positions[1];
                axis0.Normalize();

                var oldPoint = positions[0];
                var oldUnitX = GetWidthVector(sizes[0], ref invViewX, ref invViewY, ref invViewZ, ref axis0, ref axis0);

                // Step 2 - Draw each particle, connecting it to the previous (front) position

                for (int i = 0; i < lastParticle; i++)
                {
                    var centralPos = positions[i];

                    var particleSize = sizes[i];

                    // Directions for smoothing
                    var axis1 = (i + 1 < lastParticle) ? positions[i] - positions[i + 1] : positions[lastParticle - 2] - positions[lastParticle - 1];
                    axis1.Normalize();

                    var unitX = GetWidthVector(particleSize, ref invViewX, ref invViewY, ref invViewZ, ref axis0, ref axis1);

                    axis0 = axis1;

                    // Particle rotation - IGNORED for ribbon

                    // TODO Handle particle color properly - not in tiles

                    // TODO - Several texture coordinate options, including based on distance

                    var particlePos = oldPoint - oldUnitX;
                    var uvCoord = new Vector2(0, 0);
                    // 0f 0f
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();


                    // 1f 0f
                    particlePos += oldUnitX * 2;
                    uvCoord.X = 1;
                    vtxBuilder.SetAttribute(posAttribute, (IntPtr)(&particlePos));
                    vtxBuilder.SetAttribute(texAttribute, (IntPtr)(&uvCoord));
                    vtxBuilder.NextVertex();

                    particlePos += centralPos - oldPoint;
                    particlePos += unitX - oldUnitX;

                    // 1f 1f
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

                    oldUnitX = unitX;
                    oldPoint = centralPos;
                }
            }
        }
    }
}
