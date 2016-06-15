// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Particles.ShapeBuilders;

namespace CustomParticles.Particles.ShapeBuilders
{
    [DataContract("CustomParticleShape")]
    [Display("CustomShape")]
    public class CustomParticleShape : ShapeBuilder
    {
        [DataMember(100)]
        public bool FixYAxis = false;

        //  One particle requires 1 quad = 4 vertices (6 indices)
        public override int QuadsPerParticle { get; protected set; } = 1;

        /// <inheritdoc />
        public unsafe override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY,
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Step 1 - get all required fields to build the particle shapes. Some fields may not exist if no initializer or updater operates on them
            //  In that case we just decide on a default value for that field and skip the update

            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0; // We can't display the particles without position. All other fields are optional

            var sizeField = sorter.GetField(ParticleFields.Size);
            var angleField = sorter.GetField(ParticleFields.Angle);
            var hasAngle = angleField.IsValid();

            var rectField = sorter.GetField(CustomParticleFields.RectangleXY);
            var isRectangle = rectField.IsValid();

            // In case of Local space particles they are simulated in local emitter space, but drawn in world space
            //  If the draw space is identity (i.e. simulation space = draw space) skip transforming the particle's location later
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(new Quaternion(0, 0, 0, 1)));

            // Custom feature - fix the Y axis to always point up in world space rather than screen space
            if (FixYAxis)
            {
                invViewY = new Vector3(0, 1, 0);
                invViewX.Y = 0;
                invViewX.Normalize();
            }

            var renderedParticles = 0;

            var posAttribute = vtxBuilder.GetAccessor(VertexAttributes.Position);
            var texAttribute = vtxBuilder.GetAccessor(vtxBuilder.DefaultTexCoords);

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

                var unitX = invViewX * particleSize;
                var unitY = invViewY * particleSize;
                if (isRectangle)
                {
                    var rectSize = particle.Get(rectField);

                    unitX *= rectSize.X;
                    unitY *= rectSize.Y;
                }

                // Particle rotation. Positive value means clockwise rotation.
                if (hasAngle)
                {
                    var rotationAngle = particle.Get(angleField);
                    var cosA = (float)Math.Cos(rotationAngle);
                    var sinA = (float)Math.Sin(rotationAngle);
                    var tempX = unitX * cosA - unitY * sinA;
                    unitY = unitY * cosA + unitX * sinA;
                    unitX = tempX;
                }


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

            // Return the number of updated vertices
            var vtxPerShape = 4 * QuadsPerParticle;
            return renderedParticles * vtxPerShape;
        }
    }
}
