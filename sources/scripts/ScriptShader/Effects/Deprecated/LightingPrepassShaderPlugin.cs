// Copyright (c) 2011 Silicon Studio

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

using Buffer = SiliconStudio.Xenko.Graphics.Buffer;
using PrimitiveType = SiliconStudio.Xenko.Graphics.PrimitiveType;

namespace SiliconStudio.Xenko.Rendering
{
    internal class LightingPrepassShaderPlugin : ShaderPlugin<LightingPrepassPlugin>
    {
        public static readonly ParameterKey<LightData> LightInfos = ParameterKeys.ArrayValue(new LightData[64]);
        public static readonly ParameterKey<int> LightCount = LightPrepassKeys.LightCount;
        public static readonly ParameterKey<int> TileIndex = LightPrepassKeys.TileIndex;

        [ThreadStatic]
        private static LightData[] currentTiles;


        public LightingPrepassShaderPlugin()
        {
        }

        public LightingPrepassShaderPlugin(string name)
            : base(name)
        {
        }

        public bool Debug { get; set; }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            // Use the standard or debug shader
            var lightPrepassShader = new ShaderClassSource(string.Format("LightPrepass{0}", Debug ? "Debug" : string.Empty));
            DefaultShaderPass.Shader.Mixins.Add(lightPrepassShader);
            DefaultShaderPass.Shader.Compositions.Add("DiffuseColor", new ShaderClassSource("ComputeBRDFColorFresnel"));
            DefaultShaderPass.Shader.Compositions.Add("DiffuseLighting", new ShaderClassSource("ComputeBRDFDiffuseLambert"));
            DefaultShaderPass.Shader.Compositions.Add("SpecularColor", new ShaderClassSource("ComputeBRDFColor"));
            DefaultShaderPass.Shader.Compositions.Add("SpecularLighting", new ShaderClassSource("ComputeBRDFColorSpecularBlinnPhong"));
        }

        public override void SetupResources(EffectMesh effectMesh)
        {
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

            // "LightPrePassAdditiveBlend"
            Effect.Parameters.Set(BlendStateKey, BlendState.New(GraphicsDevice, blendStateDesc));

            if (Debug)
            {
                var rasterizer = RasterizerState.New(GraphicsDevice, new RasterizerStateDescription() { FillMode = FillMode.Wireframe });
                rasterizer.Name = "LightPrePassWireFrame";
                Effect.Parameters.Set(RasterizerStateKey, rasterizer);
            }

            Effect.PrepareMesh += SetupMeshResources;
            Effect.UpdateMeshData += UpdateMeshResources;
        }

        private void SetupMeshResources(EffectOld effect, EffectMesh effectMesh)
        {
            // Generates a quad for post effect rendering (should be utility function)
            var vertices = new[]
            {
                -1.0f,  1.0f, 
                 1.0f,  1.0f,
                -1.0f, -1.0f, 
                 1.0f, -1.0f,
            };

            // Use the quad for this effectMesh
            effectMesh.MeshData.Draw = new MeshDraw
                {
                    DrawCount = 4,
                    PrimitiveType = PrimitiveType.TriangleStrip,
                    VertexBuffers = new[]
                            {
                                new VertexBufferBinding(Buffer.Vertex.New(GraphicsDevice, vertices), new VertexDeclaration(VertexElement.Position<Vector2>()), 4)
                            }
                };
        }
        
        private void UpdateMeshResources(EffectOld effect, EffectMesh effectMesh)
        {
            var oldStartPass = effectMesh.Render;

            // TODO: Integrate StageStatus.Apply in API
            // Temporarely, use AddEnd as StageStatus.Apply is appended to AddStart after this code is executed
            effectMesh.Render.Set = (context) =>
                {
                    int tileIndex = context.Parameters.Get(TileIndex);
                    var tiles = RenderPassPlugin.Tiles[tileIndex];

                    var mainParameters = RenderPassPlugin.GBufferPlugin.MainPlugin.ViewParameters;

                    Matrix projMatrix;
                    mainParameters.Get(TransformationKeys.Projection, out projMatrix);
                    context.Parameters.Set(TransformationKeys.Projection, projMatrix);

                    // Use depth buffer generated by GBuffer pass
                    context.Parameters.Set(RenderTargetKeys.DepthStencilSource, RenderPassPlugin.GBufferPlugin.DepthStencil.Texture);

                    if (currentTiles == null)
                        currentTiles = new LightData[LightingPrepassPlugin.MaxLightsPerTileDrawCall];

                    //((Xenko.Framework.Graphics.Direct3D.GraphicsDevice)context.GraphicsDevice).NativeDeviceContext.InputAssembler.InputLayout = null;

                    for (int i = 0; i < (tiles.Count + LightingPrepassPlugin.MaxLightsPerTileDrawCall - 1) / LightingPrepassPlugin.MaxLightsPerTileDrawCall; ++i)
                    {
                        int lightCount = Math.Min(tiles.Count - i * LightingPrepassPlugin.MaxLightsPerTileDrawCall, LightingPrepassPlugin.MaxLightsPerTileDrawCall);
                        //effectMesh.Parameters.Set(LightCount, lightCount);
                        //effectMesh.Parameters.Set(LightInfos, RenderPassPlugin.Tiles[tileIndex].Skip(i * LightingPrepassPlugin.MaxLightsPerTileDrawCall).Take(lightCount).ToArray());

                        var startLightIndex = i * LightingPrepassPlugin.MaxLightsPerTileDrawCall;
                        for (int lightIndex = 0; lightIndex < lightCount; ++lightIndex)
                            currentTiles[lightIndex] = RenderPassPlugin.Tiles[tileIndex][startLightIndex + lightIndex];
                        context.Parameters.Set(LightCount, lightCount);
                        context.Parameters.Set(LightInfos, currentTiles);

                        // Render this tile
                        oldStartPass.Invoke(context);
                    }
                };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct LightData
        {
            public Vector3 LightPosVS;
            public float LightRadius;

            public Color3 DiffuseColor;
            public float LightIntensity;

        }
    }
}