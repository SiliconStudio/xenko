using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Particles.Sorters;
using SiliconStudio.Xenko.Particles.VertexLayouts;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    [DataContract("ShapeBuilderCommon")]
    public abstract class ShapeBuilderCommon : ShapeBuilder
    {
        public override int BuildVertexBuffer(ParticleVertexBuilder vtxBuilder, Vector3 invViewX, Vector3 invViewY, ref Vector3 spaceTranslation, ref Quaternion spaceRotation, float spaceScale, ParticleSorter sorter)
        {
            SamplerPosition?.UpdateChanges();

            SamplerSize?.UpdateChanges();

            return 0;
        }

        [DataMember(100)]
        [Display("Additive Position Animation")]
        public ComputeCurveSampler<Vector3> SamplerPosition { get; set; }

        protected unsafe Vector3 GetParticlePosition(Particle particle, ParticleFieldAccessor<Vector3> positionField, ParticleFieldAccessor<float> lifeField)
        {
            if (SamplerPosition == null)
                return particle.Get(positionField);

            var life = 1f - (*((float*)particle[lifeField]));   // The Life field contains remaining life, so for sampling we take (1 - life)

            return particle.Get(positionField) + SamplerPosition.Evaluate(life);
        }

        /// <summary>
        /// Additive animation for the particle size. If present, particle's own size will be multiplied with the sampled curve value
        /// </summary>
        /// <userdoc>
        /// Additive animation for the particle size. If present, particle's own size will be multiplied with the sampled curve value
        /// </userdoc>
        [DataMember(200)]
        [Display("Additive Size Animation")]
        public ComputeCurveSampler<float> SamplerSize { get; set; }

        protected unsafe float GetParticleSize(Particle particle, ParticleFieldAccessor<float> sizeField, ParticleFieldAccessor<float> lifeField)
        {
            var particleSize = sizeField.IsValid() ? particle.Get(sizeField) : 1f;

            if (SamplerSize == null)
                return particleSize;

            var life = 1f - (*((float*)particle[lifeField]));   // The Life field contains remaining life, so for sampling we take (1 - life)

            return particleSize * SamplerSize.Evaluate(life);
        }

    }
}
