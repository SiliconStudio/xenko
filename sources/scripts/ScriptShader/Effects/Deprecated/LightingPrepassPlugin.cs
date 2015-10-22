// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Deffered lighting plugin.
    /// </summary>
    public class LightingPrepassPlugin : RenderPassPlugin
    {
        private EffectOld[] lightDeferredEffects = new EffectOld[2];
        private RenderPass debugRenderPass;
        private IGraphicsDeviceService graphicsDeviceService;

        internal List<LightingPrepassShaderPlugin.LightData>[] Tiles;

        public const int TileCountX = 16;
        public const int TileCountY = 10;
        public const int MaxLightsPerTileDrawCall = 64;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightingPrepassPlugin"/> class.
        /// </summary>
        public LightingPrepassPlugin() : this("DefferedLighting")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LightingPrepassPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public LightingPrepassPlugin(string name) : base(name)
        {
        }

        internal RenderTarget LightTexture { get; set; }

        public EffectOld Lights { get; private set; }

        public string[] BasePlugins { get; set; }

        public GBufferPlugin GBufferPlugin { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            graphicsDeviceService = Services.GetSafeServiceAs<IGraphicsDeviceService>();
        }
        
        public override void Load()
        {
            base.Load();

            Parameters.AddSources(GBufferPlugin.MainPlugin.ViewParameters);

            Lights = this.EffectSystemOld.BuildEffect("Lights").Using(new LightPlugin()).InstantiatePermutation();

            // TODO: Check if released properly.
            for (int i = 0; i < 2; ++i)
            {
                if (i == 1)
                {
                    if (!Debug)
                        continue;

                    // Add the debug as an overlay on the main pass
                    debugRenderPass = new RenderPass("LightPrePassDebug").KeepAliveBy(ActiveObjects);
                    GBufferPlugin.MainPlugin.RenderPass.AddPass(debugRenderPass);
                }

                var debug = i == 1;
                var renderPass = i == 1 ? debugRenderPass : RenderPass;

                var lightDeferredEffectBuilder = this.EffectSystemOld.BuildEffect("LightPrePass" + (debug ? "Debug" : string.Empty)).KeepAliveBy(ActiveObjects);
                foreach (var effectPlugin in BasePlugins)
                {
                    lightDeferredEffectBuilder.Using(new BasicShaderPlugin(effectPlugin) { Services = Services, RenderPassPlugin = this, RenderPass = renderPass });
                }
                lightDeferredEffectBuilder.Using(new LightingPrepassShaderPlugin("LightPreShaderPass" + (debug ? "Debug" : string.Empty)) { Services = Services, RenderPassPlugin = this, RenderPass = renderPass, Debug = debug });

                lightDeferredEffects[i] = lightDeferredEffectBuilder.InstantiatePermutation().KeepAliveBy(ActiveObjects);
            }

            if (OfflineCompilation)
                return;

            // Create lighting accumulation texture (that LightPlugin will use)
            var mainBackBuffer = graphicsDeviceService.GraphicsDevice.BackBuffer;
            var lightTexture = Texture.New2D(GraphicsDevice, mainBackBuffer.Width, mainBackBuffer.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            lightTexture.Name = "LightTexture";
            LightTexture = lightTexture.ToRenderTarget();

            // Set Parameters for this plugin
            Parameters.Set(LightDeferredShadingKeys.LightTexture, lightTexture);

            // Set GBuffer Texture0
            Parameters.Set(GBufferBaseKeys.GBufferTexture, (Texture2D)GBufferPlugin.RenderTarget.Texture);

            // Set parameters for MainPlugin
            GBufferPlugin.MainTargetPlugin.Parameters.Set(LightDeferredShadingKeys.LightTexture, lightTexture);

            CreatePrePassMesh(RenderPass, false);
            if (Debug)
                CreatePrePassMesh(debugRenderPass, true);
        }

        public override void Unload()
        {
            GBufferPlugin.MainPlugin.RenderPass.RemovePass(debugRenderPass);
            debugRenderPass = null;

            base.Unload();
        }

        private void CreatePrePassMesh(RenderPass renderPass, bool debug)
        {
            var lightDeferredEffect = lightDeferredEffects[debug ? 1 : 0];

            Tiles = new List<LightingPrepassShaderPlugin.LightData>[TileCountX * TileCountY];
            for (int i = 0; i < Tiles.Length; ++i)
            {
                Tiles[i] = new List<LightingPrepassShaderPlugin.LightData>();
            }

            renderPass.StartPass.AddLast = (threadContext) =>
            {
                // TODO THIS IS NOT ACCURATE TO TAKE THE CURRENT BACKBUFFER
                var mainBackBuffer = graphicsDeviceService.GraphicsDevice.BackBuffer;
                threadContext.GraphicsDevice.SetViewport(new Viewport(0, 0, mainBackBuffer.Width, mainBackBuffer.Height));
                if (threadContext.FirstContext)
                {
                    if (debug)
                    {
                        threadContext.GraphicsDevice.SetRenderTarget(GBufferPlugin.MainTargetPlugin.RenderTarget);
                    }
                    else
                    {
                        threadContext.GraphicsDevice.Clear(LightTexture, new Color(0.0f, 0.0f, 0.0f, 0.0f));
                        threadContext.GraphicsDevice.SetRenderTarget(LightTexture);
                    }
                }

                for (int i = 0; i < Tiles.Length; ++i)
                    Tiles[i].Clear();

                var lights = Lights;
                var lightAttenuationCutoff = 0.1f;

                Matrix viewMatrix;
                var mainParameters = GBufferPlugin.MainPlugin.ViewParameters;
                mainParameters.Get(TransformationKeys.View, out viewMatrix);
                Matrix projMatrix;
                mainParameters.Get(TransformationKeys.Projection, out projMatrix);

                for (int index = 0; index < lights.Meshes.Count; index++)
                {
                    LightingPrepassShaderPlugin.LightData lightData;

                    var lightMesh = lights.Meshes[index];
                    Vector3 lightPos;
                    lightMesh.Parameters.TryGet(LightKeys.LightPosition, out lightPos);
                    Vector3.TransformCoordinate(ref lightPos, ref viewMatrix, out lightData.LightPosVS);
                    lightMesh.Parameters.TryGet(LightKeys.LightColor, out lightData.DiffuseColor);
                    lightMesh.Parameters.TryGet(LightKeys.LightIntensity, out lightData.LightIntensity);
                    lightMesh.Parameters.TryGet(LightKeys.LightRadius, out lightData.LightRadius);

                    // ------------------------------------------------------------------------------------------
                    // TEMPORARY FIX FOR DEFERRED LIGHTS
                    // ------------------------------------------------------------------------------------------
                    //lightData2[index].DiffuseColor.Pow(1 / 4.0f);
                    //lightData2[index].LightIntensity = (float)Math.Pow(lightData2[index].LightIntensity, 1.0f / 2.2f);
                    // ------------------------------------------------------------------------------------------

                    // Linearize color
                    lightData.DiffuseColor.Pow(Color.DefaultGamma);
                    lightData.LightIntensity = (float)Math.Pow(lightData.LightIntensity, Color.DefaultGamma);

                    float lightDistanceMax = CalculateMaxDistance(lightData.LightIntensity, lightData.LightRadius, lightAttenuationCutoff);
                    var clipRegion = ComputeClipRegion(lightData.LightPosVS, lightDistanceMax, ref projMatrix);

                    var tileStartX = (int)((clipRegion.X * 0.5f + 0.5f) * TileCountX);
                    var tileEndX = (int)((clipRegion.Z * 0.5f + 0.5f) * TileCountX);
                    var tileStartY = (int)((clipRegion.Y * 0.5f + 0.5f) * TileCountY);
                    var tileEndY = (int)((clipRegion.W * 0.5f + 0.5f) * TileCountY);

                    // Check if this light is really visible (not behind us)
                    if (lightData.LightPosVS.Z + lightDistanceMax < 0.0f)
                        continue;

                    for (int y = tileStartY; y <= tileEndY; ++y)
                    {
                        if (y < 0 || y >= TileCountY)
                            continue;
                        for (int x = tileStartX; x <= tileEndX; ++x)
                        {
                            if (x < 0 || x >= TileCountX)
                                continue;
                            Tiles[y * TileCountX + x].Add(lightData);
                        }
                    }
                }
            };

            var lightDeferredMesh = new EffectMesh(lightDeferredEffect).KeepAliveBy(ActiveObjects);
            RenderSystem.GlobalMeshes.AddMesh(lightDeferredMesh);

            renderPass.EndPass.AddLast = (context) =>
                {
                    // Clear thread context overridden variables.
                    context.Parameters.Reset(LightingPrepassShaderPlugin.LightCount);
                    context.Parameters.Reset(LightingPrepassShaderPlugin.LightInfos);
                    context.Parameters.Reset(LightingPrepassShaderPlugin.TileIndex);
                    context.Parameters.Reset(TransformationKeys.Projection);
                    context.Parameters.Reset(RenderTargetKeys.DepthStencilSource);
                };

            var tileRenderPasses = new RenderPass[TileCountX * TileCountY];
            for (int i = 0; i < tileRenderPasses.Length; ++i)
            {
                int tileIndex = i;
                tileRenderPasses[i] = new RenderPass("Lighting Tile");
                tileRenderPasses[i].StartPass.AddLast = (context) => { context.Parameters.Set(LightingPrepassShaderPlugin.TileIndex, tileIndex); };
                throw new NotImplementedException();
                //tileRenderPasses[i].Meshes.Add(lightDeferredMesh);
            }

            throw new NotImplementedException();
            //renderPass.UpdatePasses += (RenderPass currentRenderPass, ref FastList<RenderPass> currentPasses) =>
            //    {
            //        lightDeferredEffect.Passes[0].Passes.Clear();
            //        lightDeferredEffect.Passes[0].Passes.AddRange(tileRenderPasses);
            //    };
        }

        float CalculateMaxDistance(float lightIntensity, float lightRadius, float cutoff)
        {
            // DMax = r + r * (sqrt(Li/Ic) - 1) = r * sqrt(Li/Ic)
            return (float)(lightRadius * Math.Sqrt(lightIntensity / cutoff));
        }

        void UpdateClipRegionRoot(float nc,          // Tangent plane x/y normal coordinate (view space)
                            float lc,          // Light x/y coordinate (view space)
                            float lz,          // Light z coordinate (view space)
                            float lightRadius,
                            float cameraScale, // Project scale for coordinate (_11 or _22 for x/y respectively)
                            ref float clipMin,
                            ref float clipMax)
        {
            float nz = (lightRadius - nc * lc) / lz;
            float pz = (lc * lc + lz * lz - lightRadius * lightRadius) /
                        (lz - (nz / nc) * lc);

            if (pz > 0.0f)
            {
                float c = -nz * cameraScale / nc;
                if (nc > 0.0f)
                {        // Left side boundary
                    clipMin = Math.Max(clipMin, c);
                }
                else
                {                          // Right side boundary
                    clipMax = Math.Min(clipMax, c);
                }
            }
        }

        void UpdateClipRegion(float lc,          // Light x/y coordinate (view space)
                                float lz,          // Light z coordinate (view space)
                                float lightRadius,
                                float cameraScale, // Project scale for coordinate (_11 or _22 for x/y respectively)
                                ref float clipMin,
                                ref float clipMax)
        {
            float rSq = lightRadius * lightRadius;
            float lcSqPluslzSq = lc * lc + lz * lz;
            float d = rSq * lc * lc - lcSqPluslzSq * (rSq - lz * lz);

            if (d > 0)
            {
                float a = lightRadius * lc;
                float b = (float)Math.Sqrt(d);
                float nx0 = (a + b) / lcSqPluslzSq;
                float nx1 = (a - b) / lcSqPluslzSq;

                UpdateClipRegionRoot(nx0, lc, lz, lightRadius, cameraScale, ref clipMin, ref clipMax);
                UpdateClipRegionRoot(nx1, lc, lz, lightRadius, cameraScale, ref clipMin, ref clipMax);
            }
        }

        private Vector4 ComputeClipRegion(Vector3 lightPosView, float lightRadius, ref Matrix projection)
        {
            // Early out with empty rectangle if the light is too far behind the view frustum
            var clipRegion = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
            //if (lightPosView.z + lightRadius >= mCameraNearFar.x) {
            var clipMin = new Vector2(-1.0f, -1.0f);
            var clipMax = new Vector2(1.0f, 1.0f);

            UpdateClipRegion(lightPosView.X, lightPosView.Z, lightRadius, projection.M11, ref clipMin.X, ref clipMax.X);
            UpdateClipRegion(lightPosView.Y, lightPosView.Z, lightRadius, projection.M22, ref clipMin.Y, ref clipMax.Y);

            clipRegion = new Vector4(clipMin.X, clipMin.Y, clipMax.X, clipMax.Y);
            //}

            return clipRegion;
        }
    }
}
