using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialTexture")]
    [Display("StaticTexture")]
    public class ParticleMaterialTexture : ParticleMaterialBase
    {
        private Texture texture = null;

        [DataMember(100)]
        [Display("Texture")]
        public Texture Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                if (value != null)
                {
                    MandatoryVariation |= ParticleEffectVariation.HasTex0;
                }
                else
                {
                    MandatoryVariation &= ~ParticleEffectVariation.HasTex0;
                }
            }
        }

        protected EffectParameterCollectionGroup ParameterCollectionGroup { get; private set; }

        public override void Setup(GraphicsDevice graphicsDevice, ParticleEffectVariation variation, Matrix viewMatrix, Matrix projMatrix)
        {
            variation |= MandatoryVariation;    // Should be the same but still

            var effect = ParticleBatch.GetEffect(graphicsDevice, variation);

            // Get or create parameter collection
            if (ParameterCollectionGroup == null || ParameterCollectionGroup.Effect != effect)
            {
                // If ParameterCollectionGroup is not specified (using default one), let's make sure it is updated to matches effect
                // It is quite inefficient if user is often switching effect without providing a matching ParameterCollectionGroup
                ParameterCollectionGroup = new EffectParameterCollectionGroup(graphicsDevice, effect, new[] { Parameters });
            }

            SetupBase(graphicsDevice);

            // This should be CB0 - view/proj matrices don't change per material
            Parameters.Set(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

            effect.Apply(graphicsDevice, ParameterCollectionGroup, applyEffectStates: false);

            if (effect.HasParameter(TexturingKeys.Texture0))
            {
                var textureUpdater = effect.GetParameterFastUpdater(TexturingKeys.Texture0);
                textureUpdater.ApplyParameter(graphicsDevice, texture);
            }

        }

        [DataMemberIgnore]
        public override ParticleVertexLayout VertexLayout { get; protected set; } = new ParticleVertexLayoutTextured();

    }
}
