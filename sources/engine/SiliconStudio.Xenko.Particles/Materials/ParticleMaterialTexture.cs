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

        // TODO: Distribution
        private Color4 colorMin = new Color4(1, 1, 1, 1);
        private Color4 colorMax = new Color4(1, 1, 1, 1);
        private bool hasIndividualColor = false;

        [DataMember(110)]
        [Display("Color Min")]
        public Color4 ColorMin
        {
            get { return colorMin; }
            set
            {
                colorMin = value;
                UpdateColorVariation();
            }
        }

        [DataMember(120)]
        [Display("Color Max")]
        public Color4 ColorMax
        {
            get { return colorMax; }
            set
            {
                colorMax = value;
                UpdateColorVariation();
            }
        }

        private void UpdateColorVariation()
        {
            if (ColorMin.Equals(colorMax))
            {
                // Particles don't have individual color - it is passed as an uniform through the constant buffer
            //    MandatoryVariation &= ~ParticleEffectVariation.HasColor;
                hasIndividualColor = false;
            }
            else
            {
                // Particles do have individual color - vertex buffer should be patched
            //    MandatoryVariation |= ParticleEffectVariation.HasColor;
                hasIndividualColor = true;
            }
        }

        protected EffectParameterCollectionGroup ParameterCollectionGroup { get; private set; }

        public override void Setup(GraphicsDevice graphicsDevice, ParticleEffectVariation variation, Matrix viewMatrix, Matrix projMatrix, Color4 color)
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

            TextureSwizzle = (texture?.Format == PixelFormat.R32_Float ||
                              texture?.Format == PixelFormat.A8_UNorm ||
                              texture?.Format == PixelFormat.BC4_UNorm) ? (uint)1 : 0;

            SetupBase(graphicsDevice);

            // This should be CB0 - view/proj matrices don't change per material
            Parameters.Set(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

            // If particles don't have individual color, we can pass the color tint as part of the uniform color scale
            if (!hasIndividualColor)
            {
                color *= colorMax;
            }
            Parameters.Set(ParticleBaseKeys.ColorScale, color);

            effect.Apply(graphicsDevice, ParameterCollectionGroup, applyEffectStates: false);

            if (effect.HasParameter(TexturingKeys.Texture0))
            {
                var textureUpdater = effect.GetParameterFastUpdater(TexturingKeys.Texture0);
                textureUpdater.ApplyParameter(graphicsDevice, texture);
            }

        }

        // TODO Make some sort of accessor or enumerator around ParticlePool which can also sort particles
        public override void PatchVertexBuffer(IntPtr vertexBuffer, ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool)
        {
            var vtxPerParticle = vtxBuilder.VerticesPerParticle;
            var numberOfParticles = Math.Min(maxVertices / vtxPerParticle, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return;

            var lifeField = pool.GetField(ParticleFields.RemainingLife);
            var randField = pool.GetField(ParticleFields.RandomSeed);

            if (!hasIndividualColor || !randField.IsValid() || !lifeField.IsValid())
                return;

            vtxBuilder.StartBuffer(vertexBuffer);

            var whiteColor = new Color4(1, 1, 1, 1);
            var renderedParticles = 0;

            // TODO Fetch sorted particles
            foreach (var particle in pool)
            {
                var randSeed = particle.Get(randField);

                uint SeedOffset = 2;

                var colorScale = Color4.Lerp(ColorMin, ColorMax, randSeed.GetFloat(RandomOffset.Offset1A + SeedOffset));
                vtxBuilder.AddColorForParticle(ref colorScale);

                vtxBuilder.NextParticle();

                maxVertices -= vtxPerParticle;

                if (++renderedParticles >= numberOfParticles)
                {
                    vtxBuilder.EndBuffer();
                    return;
                }
            }

            vtxBuilder.EndBuffer();
        }

    }
}
