using System;
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

        [DataMemberIgnore]
        protected uint TextureSwizzle = 0;

        // TODO: Distribution

        [DataMember(110)]
        [Display("Color Min")]
        public Color4 ColorMin { get; set; } = new Color4(1, 1, 1, 1);

        [DataMember(120)]
        [Display("Color Max")]
        public Color4 ColorMax { get; set; } = new Color4(1, 1, 1, 1);


        [DataMember(130)]
        [Display("Color Random Seed")]
        public UInt32 ColorRandomOffset { get; set; } = 1;
        
        public override void Setup(GraphicsDevice graphicsDevice, ParticleEffectVariation variation, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            PrepareEffect(graphicsDevice, variation);

            // This should be CB0 - view/proj matrices don't change per material
            SetParameter(ParticleBaseKeys.MatrixTransform, viewMatrix * projMatrix);

            // Texture swizzle - fi the texture is grayscale, sample it like Tex.rrrr rather than Tex.rgba
            TextureSwizzle = (texture?.Format == PixelFormat.R32_Float ||
                              texture?.Format == PixelFormat.A8_UNorm ||
                              texture?.Format == PixelFormat.BC4_UNorm) ? (uint)1 : 0;
            SetParameter(ParticleBaseKeys.RenderFlagSwizzle, TextureSwizzle);


            // If particles don't have individual color, we can pass the color tint as part of the uniform color scale
            SetParameter(ParticleBaseKeys.ColorScaleMin, color * ColorMin);
            SetParameter(ParticleBaseKeys.ColorScaleMax, color * ColorMax);
            SetParameter(ParticleBaseKeys.ColorScaleOffset, ColorRandomOffset);

            SetParameter(TexturingKeys.Texture0, texture);

            ApplyEffect(graphicsDevice);
        }

        // TODO Make some sort of accessor or enumerator around ParticlePool which can also sort particles
        public unsafe override void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool)
        {
            var vtxPerParticle = vtxBuilder.VerticesPerParticle;
            var numberOfParticles = Math.Min(maxVertices / vtxPerParticle, pool.LivingParticles);
            if (numberOfParticles <= 0)
                return;

            var lifeField  = pool.GetField(ParticleFields.RemainingLife);
            var randField  = pool.GetField(ParticleFields.RandomSeed);

            if (!randField.IsValid() || !lifeField.IsValid())
                return;

            var colorField = pool.GetField(ParticleFields.Color);
            var hasColorField = colorField.IsValid();

            var whiteColor = new Color4(1, 1, 1, 1);

            var renderedParticles = 0;

            // TODO Fetch sorted particles
            foreach (var particle in pool)
            {
                vtxBuilder.SetColorForParticle(hasColorField ? particle[colorField] : (IntPtr)(&whiteColor));

                vtxBuilder.SetLifetimeForParticle(particle[lifeField]);

                vtxBuilder.SetRandomSeedForParticle(particle[randField]);

                vtxBuilder.NextParticle();

                maxVertices -= vtxPerParticle;

                if (++renderedParticles >= numberOfParticles)
                {
                    return;
                }
            }


        }

    }
}
