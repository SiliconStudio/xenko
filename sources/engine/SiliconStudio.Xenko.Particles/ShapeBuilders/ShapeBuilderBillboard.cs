// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Shape builder which builds each particle as a camera-facing quad
    /// </summary>
    [DataContract("ShapeBuilderBillboard")]
    [Display("Billboard")]
    public class ShapeBuilderBillboard : ShapeBuilderCommon
    {
        /// <inheritdoc />
        public override int QuadsPerParticle { get; protected set; } = 1;

        /// <summary>
        /// Additive animation for the particle rotation. If present, particle's own rotation will be added to the sampled curve value
        /// </summary>
        /// <userdoc>
        /// Additive animation for the particle rotation. If present, particle's own rotation will be added to the sampled curve value
        /// </userdoc>
        [DataMember(300)]
        [Display("Additive Rotation Animation")]
        public ComputeCurveSampler<float> SamplerRotation { get; set; }


        /// <inheritdoc />
        public unsafe override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, 
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Update the curve samplers if required
            base.BuildVertexBuffer(vtxBuilder, invViewX, invViewY, ref spaceTranslation, ref spaceRotation, spaceScale, sorter);

            SamplerRotation?.UpdateChanges();



            // Get all the required particle fields
            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            var lifeField = sorter.GetField(ParticleFields.Life);
            var sizeField = sorter.GetField(ParticleFields.Size);
            var angleField = sorter.GetField(ParticleFields.Angle);
            var hasAngle = angleField.IsValid() || (SamplerRotation != null);


            // Check if the draw space is identity - in this case we don't need to transform the position, scale and rotation vectors
            var trsIdentity = (spaceScale == 1f);
            trsIdentity = trsIdentity && (spaceTranslation.Equals(new Vector3(0, 0, 0)));
            trsIdentity = trsIdentity && (spaceRotation.Equals(Quaternion.Identity));


            var renderedParticles = 0;

            var posAttribute = vtxBuilder.GetAccessor(VertexAttributes.Position);
            var texAttribute = vtxBuilder.GetAccessor(vtxBuilder.DefaultTexCoords);

            foreach (var particle in sorter)
            {
                var centralPos = GetParticlePosition(particle, positionField, lifeField);

                var particleSize = GetParticleSize(particle, sizeField, lifeField);

                if (!trsIdentity)
                {
                    spaceRotation.Rotate(ref centralPos);
                    centralPos = centralPos * spaceScale + spaceTranslation;
                    particleSize *= spaceScale;
                }

                // Use half size to make a Size = 1 result in a Billboard of 1m x 1m
                var unitX = invViewX * (particleSize * 0.5f); 
                var unitY = invViewY * (particleSize * 0.5f);

                // Particle rotation. Positive value means clockwise rotation.
                if (hasAngle)
                {
                    var rotationAngle = GetParticleRotation(particle, angleField, lifeField);

                    var cosA = (float)Math.Cos(rotationAngle);
                    var sinA = (float)Math.Sin(rotationAngle);
                    var tempX = unitX * cosA - unitY * sinA;
                    unitY = unitY * cosA + unitX * sinA;
                    unitX = tempX;
                }


                var particlePos = centralPos - unitX + unitY;
                var uvCoord = new Vector2(0, 0);
                // 0f 0f
                vtxBuilder.SetAttribute(posAttribute, (IntPtr) (&particlePos));
                vtxBuilder.SetAttribute(texAttribute, (IntPtr) (&uvCoord));
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

        /// <summary>
        /// Gets the combined rotation for the particle, adding its field value (if any) to its sampled value from the curve
        /// </summary>
        /// <param name="particle">Target particle</param>
        /// <param name="rotationField">Rotation field accessor</param>
        /// <param name="lifeField">Normalized particle life for sampling</param>
        /// <returns>Screenspace rotation in radians, positive is clockwise</returns>
        protected unsafe float GetParticleRotation(Particle particle, ParticleFieldAccessor<float> rotationField, ParticleFieldAccessor<float> lifeField)
        {
            var particleRotation = rotationField.IsValid() ? particle.Get(rotationField) : 1f;

            if (SamplerRotation == null)
                return particleRotation;

            var life = 1f - (*((float*)particle[lifeField]));   // The Life field contains remaining life, so for sampling we take (1 - life)

            return particleRotation + MathUtil.DegreesToRadians(SamplerRotation.Evaluate(life));
        }

    }
}
