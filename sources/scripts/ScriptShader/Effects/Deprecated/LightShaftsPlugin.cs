// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Light Shaft plugin.
    /// </summary>
    public class LightShaftsPlugin : RenderPassPlugin
    {
        private EffectOld lightShaftsEffect;

        private RenderPass boundingBoxPass;
        private RenderPass minMaxPass;
        private RenderPass lightShaftPass;
        private RenderPass filterUpscalePass;

        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public LightShaftsPlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of class <see cref="LightShaftsPlugin"/>.
        /// </summary>
        public LightShaftsPlugin(string name)
            : base(name)
        {
            BoundingBoxes = new List<Mesh>();
            LightColor = new Color3(1, 1, 1);
            ExtinctionFactor = 0.001f;
            ExtinctionRatio = 0.9f;
            DensityFactor = 0.01f;
            StepCount = 8;
        }

        public ShadowMap ShadowMap { get; set; }

        public Color3 LightColor
        {
            get
            {
                return Parameters.TryGet(LightKeys.LightColor);
            }
            set
            {
                Parameters.Set(LightKeys.LightColor, value);
            }
        }

        public float ExtinctionFactor
        {
            get
            {
                return Parameters.TryGet(PostEffectLightShaftsKeys.ExtinctionFactor);
            }
            set
            {
                Parameters.Set(PostEffectLightShaftsKeys.ExtinctionFactor, value);
            }
        }

        public float ExtinctionRatio
        {
            get
            {
                return Parameters.TryGet(PostEffectLightShaftsKeys.ExtinctionRatio);
            }
            set
            {
                Parameters.Set(PostEffectLightShaftsKeys.ExtinctionRatio, value);
            }
        }

        public float DensityFactor
        {
            get
            {
                return Parameters.TryGet(PostEffectLightShaftsKeys.DensityFactor);
            }
            set
            {
                Parameters.Set(PostEffectLightShaftsKeys.DensityFactor, value);
            }
        }

        public int StepCount { get; set; }


        public ParameterCollection ViewParameters { get; set; }

        public RenderTarget RenderTarget
        {
            get;
            set;
        }

        public DepthStencilBuffer DepthStencil { get; set; }

        public List<Mesh> BoundingBoxes { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            boundingBoxPass = new RenderPass("BoundingBoxPass");
            minMaxPass = new RenderPass("MinmaxPass");
            lightShaftPass = new RenderPass("LightShaftPass");
            filterUpscalePass = new RenderPass("UpscalePass");
            RenderPass.AddPass(boundingBoxPass, minMaxPass, lightShaftPass, filterUpscalePass);

            var useUpScaling = false;

            var bbRenderTargetPlugin = new RenderTargetsPlugin("BoundingBoxRenderTargetPlugin")
                {
                    EnableSetTargets = true,
                    EnableClearTarget = true,
                    RenderPass = boundingBoxPass,
                    Services = Services,
                };

            var minMaxEffectBuilder = this.EffectSystemOld.BuildEffect("MinMax")
                .Using(new MinMaxShaderPlugin("MinMaxShaderPlugin") { RenderPassPlugin = bbRenderTargetPlugin })
                .Using(new BasicShaderPlugin("TransformationWVP") { RenderPassPlugin = bbRenderTargetPlugin });


            var minmaxEffectBuilder = this.EffectSystemOld.BuildEffect("LightShaftsMinMax")
                .Using(new PostEffectSeparateShaderPlugin() { RenderPass = minMaxPass })
                .Using(new BasicShaderPlugin("ForwardShadowMapBase") { RenderPass = minMaxPass })
                .Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectMinMax", "ShadowMapUtils.shadowMapTexture", "PointSampler", 4, 4, 0.0, 1.0)) { RenderPass = minMaxPass });

            var lightShaftsEffectBuilder = this.EffectSystemOld.BuildEffect("LightShafts")
                .Using(new PostEffectSeparateShaderPlugin() { RenderPass = lightShaftPass })
                .Using(new StateShaderPlugin() { UseBlendState = !useUpScaling, RenderPass = lightShaftPass })
                //.Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectLightShafts", Debug ? 1 : 0, RenderContext.IsZReverse ? 1 : 0, StepCount)) { RenderPass = lightShaftPass });
                .Using(new BasicShaderPlugin(new ShaderClassSource("PostEffectLightShafts", Debug ? 1 : 0, false ? 1 : 0, StepCount)) { RenderPass = lightShaftPass });

            if (OfflineCompilation)
            {
                minMaxEffectBuilder.InstantiatePermutation();
                minmaxEffectBuilder.InstantiatePermutation();
                lightShaftsEffectBuilder.InstantiatePermutation();
                return;
            }

            Parameters.AddSources(ViewParameters);
            Parameters.Set(RenderTargetKeys.DepthStencilSource, DepthStencil.Texture);
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.PointClamp);

            // BoundingBox prepass
            var gbufferDesc = RenderTarget.Description;
            var bbRenderTarget = Texture.New2D(GraphicsDevice, gbufferDesc.Width / 8, gbufferDesc.Height / 8, PixelFormat.R32G32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

            // Use MinMax Plugin
            bbRenderTargetPlugin.RenderTarget = bbRenderTarget.ToRenderTarget();
            bbRenderTargetPlugin.Parameters.AddSources(Parameters);
            bbRenderTargetPlugin.Apply();

            EffectOld minMaxEffect = minMaxEffectBuilder.InstantiatePermutation();
           
            // Add meshes
            foreach (var bbMeshData in BoundingBoxes)
            {
                // Mesh for MinPass
                var bbMesh = new EffectMesh(minMaxEffect, bbMeshData).KeepAliveBy(this);
                // Add mesh
                // boundingBoxPass.AddPass(bbMesh.EffectMeshPasses[0].EffectPass);
                RenderSystem.GlobalMeshes.AddMesh(bbMesh);
            }

            // MinMax render target
            var minMaxRenderTarget = Texture.New2D(GraphicsDevice, 256, 256, PixelFormat.R32G32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            minMaxPass.Parameters = new ParameterCollection(null);
            minMaxPass.Parameters.AddSources(ShadowMap.Parameters);
            minMaxPass.Parameters.AddSources(Parameters);
            minMaxPass.Parameters.AddDynamic(PostEffectMinMaxKeys.MinMaxCoords, ParameterDynamicValue.New(LightingPlugin.CascadeTextureCoords, (ref Vector4[] cascadeTextureCoords, ref Vector4 output) =>
                {
                    output = cascadeTextureCoords[0];
                }, autoCheckDependencies: false));

            EffectOld minmaxEffect = minmaxEffectBuilder.InstantiatePermutation();

            var minMaxMesh = new EffectMesh(minmaxEffect).KeepAliveBy(this);
            minMaxMesh.Parameters.Set(RenderTargetKeys.RenderTarget, minMaxRenderTarget.ToRenderTarget());
            RenderSystem.GlobalMeshes.AddMesh(minMaxMesh);

            // Light Shafts effect
            var blendStateDesc = new BlendStateDescription();
            blendStateDesc.SetDefaults();
            blendStateDesc.AlphaToCoverageEnable = false;
            blendStateDesc.IndependentBlendEnable = false;
            blendStateDesc.RenderTargets[0].BlendEnable = true;

            blendStateDesc.RenderTargets[0].AlphaBlendFunction = BlendFunction.Add;
            blendStateDesc.RenderTargets[0].AlphaSourceBlend = Blend.One;
            blendStateDesc.RenderTargets[0].AlphaDestinationBlend = Blend.One;

            blendStateDesc.RenderTargets[0].ColorBlendFunction = BlendFunction.Add;
            blendStateDesc.RenderTargets[0].ColorSourceBlend = Blend.One;
            blendStateDesc.RenderTargets[0].ColorDestinationBlend = Blend.One;

            blendStateDesc.RenderTargets[0].ColorWriteChannels = ColorWriteChannels.All;

            var additiveBlending = BlendState.New(GraphicsDevice, blendStateDesc);
            additiveBlending.Name = "LightShaftAdditiveBlend";

            var shaftRenderTarget = useUpScaling ? Texture.New2D(GraphicsDevice, gbufferDesc.Width / 2, gbufferDesc.Height / 2, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget() : RenderTarget;
            

            lightShaftPass.Parameters = new ParameterCollection();
            lightShaftPass.Parameters.AddSources(ShadowMap.Parameters);
            lightShaftPass.Parameters.AddSources(Parameters);

            this.lightShaftsEffect = lightShaftsEffectBuilder.InstantiatePermutation();


            var mesh = new EffectMesh(lightShaftsEffect).KeepAliveBy(this);
            mesh.Parameters.Set(TexturingKeys.Texture0, minMaxRenderTarget);
            mesh.Parameters.Set(TexturingKeys.Texture1, bbRenderTarget);
            mesh.Parameters.Set(RenderTargetKeys.RenderTarget, shaftRenderTarget);

            if (!useUpScaling)
            {
                mesh.Parameters.Set(EffectPlugin.BlendStateKey, additiveBlending);
            }
            RenderSystem.GlobalMeshes.AddMesh(mesh);

            // Bilateral Gaussian filtering for up-sampling
            if (useUpScaling)
            {
                var bbRenderTargetUpScaleH = Texture.New2D(GraphicsDevice, gbufferDesc.Width, gbufferDesc.Height / 2, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                var bbRenderTargetUpScaleV = RenderTarget;
                //var bbRenderTargetUpScaleV = GraphicsDevice.RenderTarget2D.New(gbufferDesc.Width, gbufferDesc.Height, PixelFormat.HalfVector4);

                var blurEffects = new EffectOld[] {
                                                   this.EffectSystemOld.BuildEffect("BilateralGaussianFiltering")
                                                       .Using(new PostEffectSeparateShaderPlugin())
                                                       .Using(new BasicShaderPlugin( new ShaderClassSource("PostEffectBilateralGaussian", 0))),
                                                   this.EffectSystemOld.BuildEffect("BilateralGaussianFiltering")
                                                       .Using(new StateShaderPlugin() { UseBlendState = true })
                                                       .Using(new PostEffectSeparateShaderPlugin())
                                                       .Using(new BasicShaderPlugin( new ShaderClassSource("PostEffectBilateralGaussian", 1))),
                                               };

                Texture2D textureSourceH = (Texture2D)shaftRenderTarget.Texture;
                Texture2D textureSourceV = bbRenderTargetUpScaleH;
                RenderTarget renderTargetH = bbRenderTargetUpScaleH.ToRenderTarget();
                RenderTarget renderTargetV = bbRenderTargetUpScaleV;

                var blurQuadMesh = new EffectMesh[2];
                for (int i = 0; i < 2; ++i)
                {
                    blurQuadMesh[i] = new EffectMesh(blurEffects[i]).KeepAliveBy(this);
                    filterUpscalePass.AddPass(blurQuadMesh[i].EffectPass);
                    RenderSystem.GlobalMeshes.AddMesh(blurQuadMesh[i]);
                }

                blurQuadMesh[0].Parameters.Set(TexturingKeys.Texture0, textureSourceH);
                blurQuadMesh[1].Parameters.Set(TexturingKeys.Texture0, textureSourceV);
                blurQuadMesh[0].Parameters.Set(RenderTargetKeys.RenderTarget, renderTargetH);
                blurQuadMesh[1].Parameters.Set(RenderTargetKeys.RenderTarget, renderTargetV);

                // Additive blending for 2nd render target
                blurQuadMesh[1].Parameters.Set(EffectPlugin.BlendStateKey, additiveBlending);
            }
        }

        /// <summary>
        /// Offset of the shadow map.
        /// </summary>
        internal static readonly ParameterKey<Vector3> ShadowLightOffset = ParameterKeys.Value(ParameterDynamicValue.New<Vector3, ShadowMapData>(LightingPlugin.ViewProjectionArray, CalculateShadowLightOffset));

        /// <summary>
        /// Offset of the shadow map.
        /// </summary>
        internal static readonly ParameterKey<Matrix> ShadowViewProjection = ParameterKeys.Value(ParameterDynamicValue.New<Matrix, ShadowMapData>(LightingPlugin.ViewProjectionArray, CalculateShadowViewProjection));

        /// <summary>
        /// Size factor of the current shadow map texture.
        /// </summary>
        internal static readonly ParameterKey<Vector4> ShadowTextureFactor = ParameterKeys.Value(ParameterDynamicValue.New<Vector4, Vector4[]>(LightingPlugin.CascadeTextureCoords, CalculateShadowTextureFactor));

        private static void CalculateShadowViewProjection(ref ShadowMapData cascadeData, ref Matrix output)
        {
            output = cascadeData.ViewProjCaster0;
        }

        private static void CalculateShadowLightOffset(ref ShadowMapData cascadeData, ref Vector3 output)
        {
            output = cascadeData.Offset0;
        }

        private static void CalculateShadowTextureFactor(ref Vector4[] textureCoords, ref Vector4 output)
        {
            output = textureCoords[0];
        }
    }
}