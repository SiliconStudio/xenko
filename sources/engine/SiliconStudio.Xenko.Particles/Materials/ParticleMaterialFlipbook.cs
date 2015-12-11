using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Internals;
using SiliconStudio.Xenko.Particles.VertexLayouts;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Particles.Materials
{
    [DataContract("ParticleMaterialTextureFlipbook")]
    [Display("Flipbook")]
    public class ParticleMaterialTextureFlipbook : ParticleMaterialBase
    {
        [DataMemberIgnore]
        protected override string EffectName { get; set;} = "ParticleBatch";

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

        #region Flipbook

        private UInt32 xDivisions = 4;
        private UInt32 yDivisions = 4;
        private float  xStep = 0.25f;
        private float  yStep = 0.25f;
        private UInt32 totalFrames = 16;
        private UInt32 startingFrame = 0;
        private UInt32 animationSpeedOverLife = 16;

        [DataMember(200)]
        [Display("X divisions")]
        public UInt32 XDivisions
        {
            get { return xDivisions; }
            set
            {
                xDivisions = (value > 0) ? value : 1;
                xStep = (1f / xDivisions);
                totalFrames = xDivisions * yDivisions;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(240)]
        [Display("Y divisions")]
        public UInt32 YDivisions
        {
            get { return yDivisions; }
            set
            {
                yDivisions = (value > 0) ? value : 1;
                yStep = (1f / yDivisions);
                totalFrames = xDivisions * yDivisions;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(280)]
        [Display("Starting frame")]
        public UInt32 StartingFrame
        {
            get { return startingFrame; }
            set
            {
                startingFrame = value;
                startingFrame = Math.Min(startingFrame, totalFrames);
            }
        }

        [DataMember(320)]
        [Display("Animation speed")]
        public UInt32 AnimationSpeed
        {
            get { return animationSpeedOverLife; }
            set { animationSpeedOverLife = value; }
        }
        #endregion

        public override void Setup(GraphicsDevice graphicsDevice, RenderContext context, Matrix viewMatrix, Matrix projMatrix, Color4 color)
        {
            base.Setup(graphicsDevice, context, viewMatrix, projMatrix, color);


            ///////////////
            // Shader permutations parameters - shaders will change dynamically based on those parameters
            SetParameter(ParticleBaseKeys.HasTexture, texture0 != null);


            ///////////////
            // Texture swizzle - if the texture is grayscale, sample it like Tex.rrrr rather than Tex.rgba
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

           // SetParameter(ParticleBaseKeys.ComputeColor0, new ShaderClassSource("ComputeColorBlue"));

            ApplyEffect(graphicsDevice);
        }

        // TODO Make some sort of accessor or enumerator around ParticlePool which can also sort particles
        public unsafe override void PatchVertexBuffer(ParticleVertexLayout vtxBuilder, Vector3 invViewX, Vector3 invViewY, int maxVertices, ParticlePool pool)
        {
            // If you want, you can integrate the base builder here and not call it. It should result in slight speed up
            base.PatchVertexBuffer(vtxBuilder, invViewX, invViewY, maxVertices, pool);

            var lifeField = pool.GetField(ParticleFields.RemainingLife);

            if (!lifeField.IsValid())
                return;

            foreach (var particle in pool)
            {
                var normalizedTimeline = 1f - *(float*)(particle[lifeField]);

                var spriteId = startingFrame + (int) (normalizedTimeline * animationSpeedOverLife);

                var uvTransform = new Vector4((spriteId % xDivisions) * xStep, (spriteId / yDivisions) * yStep, xStep, yStep);

                for (int i = 0; i < vtxBuilder.VerticesPerParticle; i++)
                {
                    vtxBuilder.TransformUvCoords(ref uvTransform);
                    vtxBuilder.NextVertex();
                }
            }
        }
    }
}
