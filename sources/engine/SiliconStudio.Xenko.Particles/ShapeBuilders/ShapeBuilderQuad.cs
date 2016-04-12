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
    /// Shape builder which builds each particle as a up-facing quad, further rotated by the particle's rotation in 3d space
    /// </summary>
    [DataContract("ShapeBuilderQuad")]
    [Display("Quad")]
    public class ShapeBuilderQuad : ShapeBuilderCommon
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
        public ComputeCurveSampler<Quaternion> SamplerRotation { get; set; }


        /// <inheritdoc />
        public unsafe override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, 
            ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            // Update the curve samplers if required
            base.BuildVertexBuffer(vtxBuilder, invViewX, invViewY, ref spaceTranslation, ref spaceRotation, spaceScale, sorter);

            SamplerRotation?.UpdateChanges();

            // Get all required particle fields
            var positionField = sorter.GetField(ParticleFields.Position);
            if (!positionField.IsValid())
                return 0;
            var lifeField = sorter.GetField(ParticleFields.Life);
            var sizeField = sorter.GetField(ParticleFields.Size);
            var rotField = sorter.GetField(ParticleFields.Quaternion);
            var hasRotation = rotField.IsValid() || (SamplerRotation != null);


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

                var unitX = new Vector3(1, 0, 0); 
                var unitY = new Vector3(0, 0, 1); 

                if (hasRotation)
                {
                    var particleRotation = GetParticleRotation(particle, rotField, lifeField);
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

        /// <summary>
        /// Gets the combined rotation for the particle, adding its field value (if any) to its sampled value from the curve
        /// </summary>
        /// <param name="particle">Target particle</param>
        /// <param name="rotationField">Rotation field accessor</param>
        /// <param name="lifeField">Normalized particle life for sampling</param>
        /// <returns>Quaternion rotation of the quad particle, assuming flat horizontal square at neutral rotation</returns>
        protected unsafe Quaternion GetParticleRotation(Particle particle, ParticleFieldAccessor<Quaternion> rotationField, ParticleFieldAccessor<float> lifeField)
        {
            var particleRotation = rotationField.IsValid() ? particle.Get(rotationField) : Quaternion.Identity;

            if (SamplerRotation == null)
                return particleRotation;

            var life = 1f - (*((float*)particle[lifeField]));   // The Life field contains remaining life, so for sampling we take (1 - life)

            return SamplerRotation.Evaluate(life) * particleRotation;
        }
    }
}
