// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Collections.Generic;

using SiliconStudio.Xenko.BinPacking;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering
{
    public class LightingPlugin : RenderPassPlugin
    {
        // Storage for temporary variables
        [ThreadStatic] static Matrix[] projections;
        [ThreadStatic] static Matrix[] views;
        [ThreadStatic] static Matrix[] shadowsViewProj;
        [ThreadStatic] static Vector3[] points;
        [ThreadStatic] static Vector3[] directions;

        /// <summary>
        /// Light position.
        /// </summary>
        internal static readonly ParameterKey<Vector3> ShadowLightOffset = ShadowMapCasterBaseKeys.shadowLightOffset;

        /// <summary>
        /// Array[5] of intermediate ShadowMapData.
        /// </summary>
        internal static readonly ParameterKey<ShadowMapData> ViewProjectionArray = ParameterKeys.Value<ShadowMapData>();

        /// <summary>
        /// Offset of the shadow map.
        /// </summary>
        internal static readonly ParameterKey<Vector3> Offsets = ParameterKeys.ArrayValue(new Vector3[4]);

        /// <summary>
        /// Screen coordinates for shadow map region in ShadowMapTexture.
        /// </summary>
        internal static readonly ParameterKey<Vector4> CascadeTextureCoords = ParameterKeys.ArrayValue(new Vector4[4]);

        /// <summary>
        /// Screen coordinates for shadow map region in ShadowMapTexture, including border.
        /// </summary>
        internal static readonly ParameterKey<Vector4> CascadeTextureCoordsBorder = ParameterKeys.ArrayValue(new Vector4[4]);

        private RasterizerState casterRasterizerState;
        private DepthStencilState depthStencilStateZStandard;
        private List<ShadowMap> shadowMaps = new List<ShadowMap>();
        private GuillotinePacker guillotinePacker = new GuillotinePacker();
        private EffectOld[] blurEffects;

        public LightingPlugin()
        {
            AtlasSize = 2048;
            BlurCount = 1;
        }

        /// <summary>
        /// Gets or sets the main plugin this instance is attached to.
        /// </summary>
        /// <value>
        /// The main plugin.
        /// </value>
        public MainPlugin MainPlugin { get; set; }

        public int AtlasSize { get; set; }

        public int BlurCount { get; set; }

        internal static readonly ParameterKey<int> ShadowMapLightCount = ParameterKeys.Value(0);

        internal static readonly ParameterKey<ShadowMapReceiverInfo[]> ReceiverInfo = ParameterKeys.ArrayValue<ShadowMapReceiverInfo>();

        internal static readonly ParameterKey<ShadowMapReceiverVsmInfo[]> ReceiverVsmInfo = ParameterKeys.ArrayValue<ShadowMapReceiverVsmInfo>();

        /// <summary>
        /// Gets the post RenderPass.
        /// </summary>
        internal RenderPass PostPass { get; private set; }

        protected DepthStencilBuffer ShadowMapDepth { get; private set; }

        protected Texture2D ShadowMapVsm { get; private set; }

        public void AddShadowMap(ShadowMap shadowMap)
        {
            shadowMaps.Add(shadowMap);
            shadowMap.Passes = new RenderPass[shadowMap.LevelCount];
            shadowMap.Plugins = new RenderTargetsPlugin[shadowMap.LevelCount];

            for (int i = 0; i < shadowMap.Passes.Length; i++)
            {
                shadowMap.Passes[i] = new RenderPass(string.Format("Pass {0}", i)) { Parameters = new ParameterCollection(string.Format("Parameters ShadowMap {0}", i)) };
                shadowMap.Passes[i].Parameters.AddSources(MainPlugin.ViewParameters);
                shadowMap.Passes[i].Parameters.AddSources(shadowMap.CasterParameters);

                unsafe
                {
                    int currentPassIndex = i;
                    shadowMap.Passes[i].Parameters.AddDynamic(TransformationKeys.ViewProjection,
                                                              ParameterDynamicValue.New(LightingPlugin.ViewProjectionArray, (ref ShadowMapData input, ref Matrix output) =>
                                                                  {
                                                                      fixed (Matrix* vpPtr = &input.ViewProjCaster0)
                                                                      {
                                                                          output = vpPtr[currentPassIndex];
                                                                      }
                                                                  }));
                    shadowMap.Passes[i].Parameters.AddDynamic(LightingPlugin.ShadowLightOffset,
                                                              ParameterDynamicValue.New(LightingPlugin.ViewProjectionArray, (ref ShadowMapData input, ref Vector3 output) =>
                                                                  {
                                                                      fixed (Vector3* vpPtr = &input.Offset0)
                                                                      {
                                                                          output = vpPtr[currentPassIndex];
                                                                      }
                                                                  }));
                }

                shadowMap.Plugins[i] = new RenderTargetsPlugin
                    {
                        Services = Services,
                        EnableClearTarget = false,
                        EnableClearDepth = false,
                        RenderPass = shadowMap.Passes[i],
                        RenderTarget = null,
                    };
                shadowMap.Plugins[i].Apply();
            }

            RenderPass.Passes.InsertRange(0, shadowMap.Passes);

            // Dynamic value used for ViewProjectionArray key
            var dynamicViewProjectionArray = ParameterDynamicValue.New(
                TransformationKeys.View, TransformationKeys.Projection, LightKeys.LightDirection, LightKeys.LightColor, LightingPlugin.Offsets, (ref Matrix view, ref Matrix projection, ref Vector3 lightDirection, ref Color3 lightColor, ref Vector3[] offsets, ref ShadowMapData result) =>
                    {
                        if (projections == null)
                        {
                            // Preallocates temporary variables (thread static)
                            projections = new Matrix[4];
                            views = new Matrix[4];
                            shadowsViewProj = new Matrix[8];
                            points = new Vector3[8];
                            directions = new Vector3[4];
                        }

                        Matrix inverseView, inverseProjection;
                        Matrix.Invert(ref projection, out inverseProjection);
                        Matrix.Invert(ref view, out inverseView);

                        // Frustum in World Space
                        for (int i = 0; i < 8; ++i)
                            Vector3.TransformCoordinate(ref FrustrumBasePoints[i], ref inverseProjection, out points[i]);

                        for (int i = 0; i < 4; i++)
                        {
                            directions[i] = Vector3.Normalize(points[i + 4] - points[i]);
                        }

                        // TODO Make these factors configurable. They need also to be correctly tweaked.
                        float shadowDistribute = 1.0f / shadowMap.LevelCount;
                        float znear = 1.0f;
                        float zfar = shadowMap.ShadowDistance;

                        var shadowOffsets = new Vector3[shadowMap.LevelCount];
                        var boudingBoxVectors = new Vector3[shadowMap.LevelCount * 2];
                        var direction = Vector3.Normalize(lightDirection);

                        // Fake value
                        // It will be setup by next loop
                        Vector3 side = Vector3.UnitX;
                        Vector3 up = Vector3.UnitX;

                        // Select best Up vector
                        foreach (var vectorUp in VectorUps)
                        {
                            if (Vector3.Dot(direction, vectorUp) < (1.0 - 0.0001))
                            {
                                side = Vector3.Normalize(Vector3.Cross(vectorUp, direction));
                                up = Vector3.Normalize(Vector3.Cross(direction, side));
                                break;
                            }
                        }

                        for (int cascadeLevel = 0; cascadeLevel < shadowMap.LevelCount; ++cascadeLevel)
                        {
                            float k0 = (float)(cascadeLevel + 0) / shadowMap.LevelCount;
                            float k1 = (float)(cascadeLevel + 1) / shadowMap.LevelCount;
                            float min = (float)(znear * Math.Pow(zfar / znear, k0)) * (1.0f - shadowDistribute) + (znear + (zfar - znear) * k0) * shadowDistribute;
                            float max = (float)(znear * Math.Pow(zfar / znear, k1)) * (1.0f - shadowDistribute) + (znear + (zfar - znear) * k1) * shadowDistribute;

                            for (int j = 0; j < shadowMap.LevelCount; j++)
                            {
                                boudingBoxVectors[j] = points[j] + directions[j] * min;
                                boudingBoxVectors[j + shadowMap.LevelCount] = points[j] + directions[j] * max;
                            }
                            var boundingBox = BoundingBox.FromPoints(boudingBoxVectors);

                            var radius = (boundingBox.Maximum - boundingBox.Minimum).Length() * 0.5f;
                            var target = Vector3.TransformCoordinate(boundingBox.Center, inverseView);

                            // Snap camera to texel units (so that shadow doesn't jitter)
                            var shadowMapHalfSize = shadowMap.ShadowMapSize * 0.5f;
                            float x = (float)Math.Ceiling(Vector3.Dot(target, up) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                            float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                            float z = Vector3.Dot(target, direction);
                            //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                            target = up * x + side * y + direction * z;

                            views[cascadeLevel] = Matrix.LookAtLH(target - direction * zfar * 0.5f, target + direction * zfar * 0.5f, up); // View;
                            projections[cascadeLevel] = Matrix.OrthoOffCenterLH(-radius, radius, -radius, radius, znear / zfar, zfar); // Projection

                            //float leftX = shadowMap.Level == CascadeShadowMapLevel.X1 ? 0.5f : 0.25f;
                            //float leftY = shadowMap.Level == CascadeShadowMapLevel.X4 ? 0.25f : 0.5f;
                            //float centerX = 0.5f * (cascadeLevel % 2) + leftX;
                            //float centerY = 0.5f * (cascadeLevel / 2) + leftY;

                            var cascadeTextureCoords = shadowMap.TextureCoordsBorder[cascadeLevel];

                            float leftX = (float)shadowMap.ShadowMapSize / (float)AtlasSize * 0.5f;
                            float leftY = (float)shadowMap.ShadowMapSize / (float)AtlasSize * 0.5f;
                            float centerX = 0.5f * (cascadeTextureCoords.X + cascadeTextureCoords.Z);
                            float centerY = 0.5f * (cascadeTextureCoords.Y + cascadeTextureCoords.W);

                            shadowsViewProj[cascadeLevel] = views[cascadeLevel] * projections[cascadeLevel];
                            shadowsViewProj[cascadeLevel + 4] = shadowsViewProj[cascadeLevel]
                                                                * Matrix.Scaling(leftX, -leftY, 0.5f) // Texture0 mapping offsets/scaling
                                                                * Matrix.Translation(centerX, centerY, 0.5f);

                            var shadowVInverse = Matrix.Invert(views[cascadeLevel]);
                            shadowOffsets[cascadeLevel] = new Vector3(shadowVInverse.M41, shadowVInverse.M42, shadowVInverse.M43);
                        }

                        result.LightColor = lightColor;

                        unsafe
                        {
                            fixed (Matrix* resultPtr = &result.ViewProjCaster0)
                                Utilities.Write((IntPtr)resultPtr, shadowsViewProj, 0, shadowsViewProj.Length);

                            fixed (Vector3* resultPtr = &result.Offset0)
                                Utilities.Write((IntPtr)resultPtr, shadowOffsets, 0, shadowOffsets.Length);
                        }
                    });

            shadowMap.Parameters.SetDefault(LightKeys.LightDirection);
            shadowMap.Parameters.SetDefault(Offsets);
            shadowMap.Parameters.SetDefault(ViewProjectionArray);
            shadowMap.Parameters.AddDynamic(ViewProjectionArray, dynamicViewProjectionArray);
            shadowMap.CasterParameters.Set(EffectPlugin.RasterizerStateKey, null);
            shadowMap.Texture = shadowMap.Filter is ShadowMapFilterVsm ? ShadowMapVsm : ShadowMapDepth.Texture;
        }

        public void RemoveShadowMap(ShadowMap shadowMap)
        {
            shadowMaps.Remove(shadowMap);
            RenderPass.Passes.RemoveAll(shadowMap.Passes.Contains);
        }

        public void UpdateShadowMaps(ThreadContext context)
        {
            guillotinePacker.Clear(ShadowMapDepth.Description.Width, ShadowMapDepth.Description.Height);

            // Allocate shadow maps in the atlas and update texture coordinates.
            foreach (var shadowMap in shadowMaps)
            {
                //int widthFactor = shadowMap.Level == CascadeShadowMapLevel.X1 ? 1 : 2;
                //int heightFactor = shadowMap.Level == CascadeShadowMapLevel.X4 ? 2 : 1;

                //var shadowMapWidth = shadowMap.ShadowMapSize * widthFactor;
                //var shadowMapHeight = shadowMap.ShadowMapSize * heightFactor;

                var cascadeTextureCoords = new Vector4[shadowMap.LevelCount];
                var cascadeTextureCoordsBorder = new Vector4[shadowMap.LevelCount];
                for (int i = 0; i < shadowMap.LevelCount; ++i)
                {
                    var rect = guillotinePacker.Insert(shadowMap.ShadowMapSize, shadowMap.ShadowMapSize);

                    // Texture0 array support should help when this break (in complex scenes)
                    if (rect.Width == 0)
                        throw new InvalidOperationException("Could not allocate enough texture space for shadow map texture.");

                    Vector4 cascadeTextureCoord;
                    cascadeTextureCoord.X = (float)(rect.X) / guillotinePacker.Width;
                    cascadeTextureCoord.Y = (float)(rect.Y) / guillotinePacker.Height;
                    cascadeTextureCoord.Z = (float)(rect.X + rect.Width) / guillotinePacker.Width;
                    cascadeTextureCoord.W = (float)(rect.Y + rect.Height) / guillotinePacker.Height;

                    cascadeTextureCoords[i] = cascadeTextureCoord;

                    cascadeTextureCoord.X += 0.01f;
                    cascadeTextureCoord.Y += 0.01f;
                    cascadeTextureCoord.Z -= 0.01f;
                    cascadeTextureCoord.W -= 0.01f;

                    cascadeTextureCoordsBorder[i] = cascadeTextureCoord;

                    shadowMap.Plugins[i].Viewport = new Viewport(rect.X, rect.Y, shadowMap.ShadowMapSize, shadowMap.ShadowMapSize);
                    shadowMap.Plugins[i].DepthStencil = ShadowMapDepth;
                }

                shadowMap.Parameters.Set(CascadeTextureCoords, cascadeTextureCoords);
                shadowMap.Parameters.Set(CascadeTextureCoordsBorder, cascadeTextureCoordsBorder);

                shadowMap.CasterParameters.Set(EffectPlugin.RasterizerStateKey, casterRasterizerState);
                shadowMap.CasterParameters.Set(EffectPlugin.DepthStencilStateKey, depthStencilStateZStandard);

                shadowMap.TextureCoordsBorder = cascadeTextureCoordsBorder;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            blurEffects = new EffectOld[]
                {
                    this.EffectSystemOld.BuildEffect("VsmBlurH")
                        .Using(new PostEffectSeparateShaderPlugin())
                        .Using(new BasicShaderPlugin("PostEffectBlurHVsm"))
                        .KeepAliveBy(this),

                    this.EffectSystemOld.BuildEffect("VsmBlurV")
                        .Using(new PostEffectSeparateShaderPlugin())
                        .Using(new BasicShaderPlugin("PostEffectBlur"))
                        .KeepAliveBy(this)
                }; 
            
            if (OfflineCompilation)
                return;

            RenderSystem.GlobalPass.StartPass += UpdateShadowMaps;
        }

        protected override void Destroy()
        {
            base.Destroy();

            if (OfflineCompilation)
                return;

            RenderSystem.GlobalPass.StartPass -= UpdateShadowMaps;
        }

        public override void Load()
        {
            base.Load();

            if (OfflineCompilation)
                return;

            // Declare post render pass
            PostPass = new RenderPass("PostPass").KeepAliveBy(ActiveObjects);
            RenderPass.AddPass(PostPass);


            var depthStencilTexture = Texture.New2D(GraphicsDevice, AtlasSize, AtlasSize, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource).KeepAliveBy(ActiveObjects);
            var depthStencilBuffer = depthStencilTexture.ToDepthStencilBuffer(false);
            ShadowMapDepth = depthStencilBuffer;

            //MainTargetPlugin.Parameters.Set(ShadowMapKeys.Texture0, ShadowMapDepth);

            // Setup clear of this target
            var renderTargetPlugin = new RenderTargetsPlugin
                {
                    Services = Services,
                    EnableClearDepth = true,
                    EnableSetTargets = false,
                    RenderPass = RenderPass,
                    RenderTarget = null,
                    DepthStencil = depthStencilBuffer,
                };
            renderTargetPlugin.Apply();

            // Use Default ZTest for GBuffer
            depthStencilStateZStandard = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription().Default()).KeepAliveBy(ActiveObjects);
            depthStencilStateZStandard.Name = "ZStandard";

            Parameters.Set(EffectPlugin.DepthStencilStateKey, depthStencilStateZStandard);

            casterRasterizerState = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.Back)).KeepAliveBy(ActiveObjects);

            // Variance Shadow Mapping
            // Create the blur temporary texture
            var shadowMapTextureDesc = ShadowMapDepth.Description;
            var shadowMapBlurH = Texture.New2D(GraphicsDevice, shadowMapTextureDesc.Width, shadowMapTextureDesc.Height, PixelFormat.R32G32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).KeepAliveBy(ActiveObjects);
            var shadowMapBlurV = Texture.New2D(GraphicsDevice, shadowMapTextureDesc.Width, shadowMapTextureDesc.Height, PixelFormat.R32G32_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).KeepAliveBy(ActiveObjects);

            Texture2D textureSourceH = ShadowMapDepth.Texture;
            Texture2D textureSourceV = shadowMapBlurH;
            RenderTarget renderTargetH = shadowMapBlurH.ToRenderTarget();
            RenderTarget renderTargetV = shadowMapBlurV.ToRenderTarget();

            var blurQuadMesh = new EffectMesh[2];
            for (int j = 0; j < BlurCount; j++)
            {
                for (int i = 0; i < 2; ++i)
                {
                    blurQuadMesh[i] = new EffectMesh(j > 0 ? blurEffects[1] : blurEffects[i]).KeepAliveBy(ActiveObjects);
                    blurQuadMesh[i].Parameters.Set(PostEffectBlurKeys.Coefficients, new[] { 0.2270270270f, 0.3162162162f, 0.3162162162f, 0.0702702703f, 0.0702702703f });
                    var unit = i == 0 ? Vector2.UnitX : Vector2.UnitY;
                    blurQuadMesh[i].Parameters.Set(PostEffectBlurKeys.Offsets, new[] { Vector2.Zero, unit * -1.3846153846f, unit * +1.3846153846f, unit * -3.2307692308f, unit * +3.2307692308f });

                    PostPass.AddPass(blurQuadMesh[i].EffectPass);

                    RenderSystem.GlobalMeshes.AddMesh(blurQuadMesh[i]);
                }

                blurQuadMesh[0].Parameters.Set(TexturingKeys.Texture0, textureSourceH);
                blurQuadMesh[1].Parameters.Set(TexturingKeys.Texture0, textureSourceV);
                blurQuadMesh[0].Parameters.Set(RenderTargetKeys.RenderTarget, renderTargetH);
                blurQuadMesh[1].Parameters.Set(RenderTargetKeys.RenderTarget, renderTargetV);

                textureSourceH = shadowMapBlurV;
                textureSourceV = shadowMapBlurH;
            }

            ShadowMapVsm = shadowMapBlurV;

            // Final texture for VSM is result of blur
            //MainTargetPlugin.Parameters.Set(ShadowMapKeys.Texture0, shadowMapBlurV);
        }

        public override void Unload()
        {
            RenderPass.RemovePass(PostPass);

            base.Unload();
        }

        /// <summary>
        /// Base points for frustrum
        /// </summary>
        private static readonly Vector3[] FrustrumBasePoints =
            {
                new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f),
                new Vector3(-1.0f,-1.0f, 1.0f), new Vector3(1.0f,-1.0f, 1.0f), new Vector3(-1.0f,1.0f, 1.0f), new Vector3(1.0f,1.0f, 1.0f),
            };

        private static readonly Vector3[] VectorUps = new[] { Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX };
    }
}
