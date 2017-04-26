// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Background;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Materials;
using SiliconStudio.Xenko.Rendering.Shadows;
using SiliconStudio.Xenko.Rendering.Sprites;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public static class GraphicsCompositorHelper
    {
        /// <summary>
        /// Creates a typical graphics compositor programatically. It can render meshes, sprites and backgrounds.
        /// </summary>
        public static GraphicsCompositor CreateDefault(bool enablePostEffects, string modelEffectName = "XenkoForwardShadingEffect", CameraComponent camera = null, Color4? clearColor = null, GraphicsProfile graphicsProfile = GraphicsProfile.Level_10_0)
        {
            var opaqueRenderStage = new RenderStage("Opaque", "Main") { SortMode = new StateChangeSortMode() };
            var transparentRenderStage = new RenderStage("Transparent", "Main") { SortMode = new BackToFrontSortMode() };
            var shadowCasterRenderStage = new RenderStage("ShadowMapCaster", "ShadowMapCaster") { SortMode = new FrontToBackSortMode() };
            var shadowCasterCubeMapRenderStage = new RenderStage("ShadowMapCasterCubeMap", "ShadowMapCasterCubeMap") { SortMode = new FrontToBackSortMode() };
            var shadowCasterParaboloidRenderStage = new RenderStage("ShadowMapCasterParaboloid", "ShadowMapCasterParaboloid") { SortMode = new FrontToBackSortMode() };

            var postProcessingEffects = enablePostEffects
                ? new PostProcessingEffects
                {
                    ColorTransforms =
                    {
                        Transforms =
                        {
                            new ToneMap()
                        },
                    },
                }
                : null;

            if (postProcessingEffects != null)
            {
                postProcessingEffects.DisableAll();
                postProcessingEffects.ColorTransforms.Enabled = true;
            }

            var singleView = new ForwardRenderer
            {
                Clear = { Color = clearColor ?? Color.CornflowerBlue },
                OpaqueRenderStage = opaqueRenderStage,
                TransparentRenderStage = transparentRenderStage,
                ShadowMapRenderStages = { shadowCasterRenderStage, shadowCasterParaboloidRenderStage, shadowCasterCubeMapRenderStage },
                PostEffects = postProcessingEffects,
            };

            var forwardLighting = graphicsProfile >= GraphicsProfile.Level_10_0
                ? new ForwardLightingRenderFeature
                {
                    LightRenderers =
                    {
                        new LightAmbientRenderer(),
                        new LightSkyboxRenderer(),
                        new LightDirectionalGroupRenderer(),
                        new LightPointGroupRenderer(),
                        new LightSpotGroupRenderer(),
                        new LightClusteredPointSpotGroupRenderer(),
                    },
                    ShadowMapRenderer = new ShadowMapRenderer
                    {
                        Renderers =
                        {
                            new LightDirectionalShadowMapRenderer
                            {
                                ShadowCasterRenderStage = shadowCasterRenderStage
                            },
                            new LightSpotShadowMapRenderer
                            {
                                ShadowCasterRenderStage = shadowCasterRenderStage
                            },
                            new LightPointShadowMapRendererParaboloid
                            {
                                ShadowCasterRenderStage = shadowCasterParaboloidRenderStage
                            },
                            new LightPointShadowMapRendererCubeMap
                            {
                                ShadowCasterRenderStage = shadowCasterCubeMapRenderStage
                            },
                        },
                    },
                }
                : new ForwardLightingRenderFeature
                {
                    LightRenderers =
                    {
                        new LightAmbientRenderer(),
                        new LightDirectionalGroupRenderer(),
                        new LightSkyboxRenderer(),
                        new LightPointGroupRenderer(),
                        new LightSpotGroupRenderer(),
                    },
                };

            var cameraSlot = new SceneCameraSlot();
            if (camera != null)
                camera.Slot = cameraSlot.ToSlotId();

            return new GraphicsCompositor
            {
                Cameras =
                {
                    cameraSlot
                },
                RenderStages =
                {
                    opaqueRenderStage,
                    transparentRenderStage,
                    shadowCasterRenderStage,
                    shadowCasterParaboloidRenderStage,
                    shadowCasterCubeMapRenderStage,
                },
                RenderFeatures =
                {
                    new MeshRenderFeature
                    {
                        RenderFeatures =
                        {
                            new TransformRenderFeature(),
                            new SkinningRenderFeature(),
                            new MaterialRenderFeature(),
                            new ShadowCasterRenderFeature(),
                            forwardLighting,
                        },
                        RenderStageSelectors =
                        {
                            new MeshTransparentRenderStageSelector
                            {
                                EffectName = modelEffectName,
                                OpaqueRenderStage = opaqueRenderStage,
                                TransparentRenderStage = transparentRenderStage,
                            },
                            new ShadowMapRenderStageSelector
                            {
                                EffectName = modelEffectName + ".ShadowMapCaster",
                                ShadowMapRenderStage = shadowCasterRenderStage,
                            },
                            new ShadowMapRenderStageSelector
                            {
                                EffectName = modelEffectName + ".ShadowMapCasterParaboloid",
                                ShadowMapRenderStage = shadowCasterParaboloidRenderStage,
                            },
                            new ShadowMapRenderStageSelector
                            {
                                EffectName = modelEffectName + ".ShadowMapCasterCubeMap",
                                ShadowMapRenderStage = shadowCasterCubeMapRenderStage,
                            },
                        },
                        PipelineProcessors =
                        {
                            new MeshPipelineProcessor { TransparentRenderStage = transparentRenderStage },
                            new ShadowMeshPipelineProcessor { ShadowMapRenderStage = shadowCasterRenderStage },
                            new ShadowMeshPipelineProcessor { ShadowMapRenderStage = shadowCasterParaboloidRenderStage, DepthClipping = true },
                            new ShadowMeshPipelineProcessor { ShadowMapRenderStage = shadowCasterCubeMapRenderStage, DepthClipping = true },
                        }
                    },
                    new SpriteRenderFeature
                    {
                        RenderStageSelectors =
                        {
                            new SpriteTransparentRenderStageSelector
                            {
                                EffectName = "Test",
                                OpaqueRenderStage = opaqueRenderStage,
                                TransparentRenderStage = transparentRenderStage,
                            }
                        },
                    },
                    new BackgroundRenderFeature
                    {
                        RenderStageSelectors =
                        {
                            new SimpleGroupToRenderStageSelector
                            {
                                RenderStage = opaqueRenderStage,
                                EffectName = "Test",
                            }
                        },
                    },
                },
                Game = new SceneCameraRenderer()
                {
                    Child = singleView,
                    Camera = cameraSlot
                },
                Editor = singleView,
                SingleView = singleView,
            };
        }
    }
}
