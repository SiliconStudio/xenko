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
        [DataMemberIgnore]
        protected override string EffectName { get; set; } = "ParticleBatch";
        private Texture texture0 = null;

        [DataMember(100)]
        [Display("Texture")]
        public Texture Texture
        {
            get { return texture0; }
            set
            {
                texture0 = value;
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
        private TextureAddressMode address0 = TextureAddressMode.Wrap;

        [DataMember(102)]
        [Display("Address mode")]
        public TextureAddressMode AddressMode0
        {
            get { return address0; }
            set
            {
                address0 = value;
                dirtySamplerState0 = true;
            }
        }

        [DataMemberIgnore]
        private SamplerState sampler0 = null;

        [DataMemberIgnore]
        private bool dirtySamplerState0 = true;


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
        
        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);

            ///////////////
            // Shader permutations parameters - shaders will change dynamically based on those parameters
            SetParameter(ParticleBaseKeys.HasTexture, texture0 != null);


            ///////////////
            // Texture swizzle - fi the texture is grayscale, sample it like Tex.rrrr rather than Tex.rgba
            TextureSwizzle = (texture0?.Format == PixelFormat.R32_Float ||
                              texture0?.Format == PixelFormat.A8_UNorm ||
                              texture0?.Format == PixelFormat.BC4_UNorm) ? (uint)1 : 0;
            SetParameter(ParticleBaseKeys.RenderFlagSwizzle, TextureSwizzle);


            // If particles don't have individual color, we can pass the color tint as part of the uniform color scale
            SetParameter(ParticleBaseKeys.ColorScaleMin, color * ColorMin);
            SetParameter(ParticleBaseKeys.ColorScaleMax, color * ColorMax);
            SetParameter(ParticleBaseKeys.ColorScaleOffset, ColorRandomOffset);

            if (sampler0 == null || dirtySamplerState0)
            {
                sampler0 = SamplerState.New(graphicsDevice, new SamplerStateDescription(TextureFilter.Linear, address0));
                dirtySamplerState0 = false;
            }
            SetParameter(TexturingKeys.Texture0, texture0);
            SetParameter(TexturingKeys.Sampler0, sampler0);

            ApplyEffect(graphicsDevice);
        }

    }
}
