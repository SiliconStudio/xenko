// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    /// <summary>
    /// Handles rendering of shadow map casters.
    /// </summary>
    public class ShadowMapRenderer
    {
        // TODO: Extract a common interface and implem for shadow renderer (not only shadow maps)

        public NextGenRenderSystem RenderSystem { get; set; }

        private readonly RenderStage shadowMapRenderStage;

        private readonly List<RenderView> shadowRenderViews = new List<RenderView>();

        private FastListStruct<ShadowMapAtlasTexture> atlases;

        private PoolListStruct<LightShadowMapTexture> shadowMapTextures;

        private readonly int MaximumTextureSize = (int)(ReferenceShadowSize * ComputeSizeFactor(LightShadowMapSize.XLarge) * 2.0f);

        private const float ReferenceShadowSize = 1024;

        public ShadowMapRenderer(NextGenRenderSystem renderSystem, RenderStage shadowMapRenderStage)
        {
            RenderSystem = renderSystem;
            this.shadowMapRenderStage = shadowMapRenderStage;

            atlases = new FastListStruct<ShadowMapAtlasTexture>(16);
            shadowMapTextures = new PoolListStruct<LightShadowMapTexture>(16, CreateLightShadowMapTexture);

            Renderers = new Dictionary<Type, ILightShadowMapRenderer>();

            ShadowCamera = new CameraComponent { UseCustomViewMatrix = true, UseCustomProjectionMatrix = true };
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        public CameraComponent Camera { get; private set; }

        /// <summary>
        /// The shadow camera used for rendering from the shadow space.
        /// </summary>
        public readonly CameraComponent ShadowCamera;

        public Dictionary<Type, ILightShadowMapRenderer> Renderers { get; }

        public ILightShadowMapRenderer FindRenderer(Type lightType)
        {
            ILightShadowMapRenderer shadowMapRenderer;
            Renderers.TryGetValue(lightType, out shadowMapRenderer);
            return shadowMapRenderer;
        }

        public void Extract(Dictionary<RenderView, ForwardLightingRenderFeature.RenderViewLightData> renderViewLightDatas)
        {
            // Cleanup previous shadow render views
            foreach (var shadowRenderView in shadowRenderViews)
                RenderSystem.Views.Remove(shadowRenderView);
            shadowRenderViews.Clear();

            // Clear currently associated shadows
            shadowMapTextures.Clear();
            
            // Reset the state of renderers
            foreach (var rendererKeyPairs in Renderers)
            {
                var renderer = rendererKeyPairs.Value;
                renderer.Reset();
            }

            foreach (var renderViewData in renderViewLightDatas)
            {
                renderViewData.Value.LightComponentsWithShadows.Clear();

                // Gets the current camera
                Camera = renderViewData.Key.Camera;

                if (Camera == null)
                {
                    continue;
                }

                // Collect all required shadow maps
                CollectShadowMaps(renderViewData.Key, renderViewData.Value);

                // No shadow maps to render
                if (shadowMapTextures.Count == 0)
                {
                    continue;
                }

                // Assign rectangles to shadow maps
                AssignRectangles();

                // Collect shadow render views
                foreach (var lightShadowMapTexture in renderViewData.Value.LightComponentsWithShadows)
                {
                    var shadowMapTexture = lightShadowMapTexture.Value;
                    shadowMapTexture.Renderer.Extract(RenderSystem.RenderContextOld, this, shadowMapTexture);
                    for (int cascadeIndex = 0; cascadeIndex < shadowMapTexture.CascadeCount; cascadeIndex++)
                    {
                        // TODO GRAPHICS REFACTOR reuse views
                        var shadowRenderView = new ShadowMapRenderView
                        {
                            RenderStages = { shadowMapRenderStage },
                            RenderView = renderViewData.Key,
                            ShadowMapTexture = shadowMapTexture,
                            Rectangle = shadowMapTexture.GetRectangle(cascadeIndex)
                        };

                        shadowMapTexture.Renderer.GetCascadeViewParameters(shadowMapTexture, cascadeIndex, out shadowRenderView.View, out shadowRenderView.Projection);

                        shadowRenderViews.Add(shadowRenderView);
                        RenderSystem.Views.Add(shadowRenderView);
                    }
                }
            }
        }

        public void ClearAtlasRenderTargets(CommandList commandList)
        {
            // Clear atlases
            foreach (var atlas in atlases)
            {
                atlas.ClearRenderTargetIfNecessary(commandList);
            }
        }

        private void AssignRectangles()
        {
            // Clear atlases
            foreach (var atlas in atlases)
            {
                atlas.Clear();
            }

            // Assign rectangles for shadowmaps
            foreach (var shadowMapTexture in shadowMapTextures)
            {
                AssignRectangles(shadowMapTexture);
            }
        }

        private void AssignRectangles(LightShadowMapTexture lightShadowMapTexture)
        {
            lightShadowMapTexture.CascadeCount = lightShadowMapTexture.Shadow.GetCascadeCount();
            var size = lightShadowMapTexture.Size;

            // Try to fit the shadow map into an existing atlas
            ShadowMapAtlasTexture currentAtlas = null;
            foreach (var atlas in atlases)
            {
                if (atlas.TryInsert(size, size, lightShadowMapTexture.CascadeCount, (int index, ref Rectangle rectangle) => lightShadowMapTexture.SetRectangle(index, rectangle)))
                {
                    currentAtlas = atlas;
                    break;
                }
            }

            // Allocate a new atlas texture
            if (currentAtlas == null)
            {
                // TODO: handle FilterType texture creation here
                // TODO: This does not work for Omni lights

                var texture = Texture.New2D(RenderSystem.GraphicsDevice, MaximumTextureSize, MaximumTextureSize, 1, shadowMapRenderStage.Output.DepthStencilFormat, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
                currentAtlas = new ShadowMapAtlasTexture(texture, atlases.Count) { FilterType = lightShadowMapTexture.FilterType };
                atlases.Add(currentAtlas);

                for (int i = 0; i < lightShadowMapTexture.CascadeCount; i++)
                {
                    var rect = Rectangle.Empty;
                    currentAtlas.Insert(size, size, ref rect);
                    lightShadowMapTexture.SetRectangle(i, rect);
                }
            }

            // Make sure the atlas cleared (will be clear just once)
            lightShadowMapTexture.TextureId = (byte)currentAtlas.Id;
            lightShadowMapTexture.Atlas = currentAtlas;
        }

        private void CollectShadowMaps(RenderView renderView, ForwardLightingRenderFeature.RenderViewLightData renderViewLightData)
        {
            // TODO GRAPHICS REFACTOR Only lights of current scene!

            var sceneCameraRenderer = renderView.SceneCameraRenderer;
            var viewport = sceneCameraRenderer.ComputedViewport;

            foreach (var lightComponent in renderViewLightData.VisibleLightsWithShadows)
            {
                var light = lightComponent.Type as IDirectLight;
                if (light == null)
                {
                    continue;
                }

                var shadowMap = light.Shadow;
                if (!shadowMap.Enabled)
                {
                    continue;
                }

                // Check if the light has a shadow map renderer
                var lightType = light.GetType();
                ILightShadowMapRenderer renderer;
                if (!Renderers.TryGetValue(lightType, out renderer))
                {
                    continue;
                }

                var direction = lightComponent.Direction;
                var position = lightComponent.Position;

                // Compute the coverage of this light on the screen
                var size = light.ComputeScreenCoverage(renderView.Camera, position, direction, viewport.Width, viewport.Height);

                // Converts the importance into a shadow size factor
                var sizeFactor = ComputeSizeFactor(shadowMap.Size);

                // Compute the size of the final shadow map
                // TODO: Handle GraphicsProfile
                var shadowMapSize = (int)Math.Min(ReferenceShadowSize * sizeFactor, MathUtil.NextPowerOfTwo(size * sizeFactor));

                if (shadowMapSize <= 0) // TODO: Validate < 0 earlier in the setters
                {
                    continue;
                }

                // Get or allocate  a ShadowMapTexture
                var shadowMapTexture = shadowMapTextures.Add();
                shadowMapTexture.Initialize(lightComponent, light, shadowMap, shadowMapSize, renderer);

                renderViewLightData.LightComponentsWithShadows.Add(lightComponent, shadowMapTexture);
            }
        }

        private static float ComputeSizeFactor(LightShadowMapSize shadowMapSize)
        {
            // Then reduce the size based on the shadow map size
            var factor = (float)Math.Pow(2.0f, (int)shadowMapSize - 3.0f);
            return factor;
        }

        private static LightShadowMapTexture CreateLightShadowMapTexture()
        {
            return new LightShadowMapTexture();
        }

        public struct LightComponentKey : IEquatable<LightComponentKey>
        {
            public readonly LightComponent LightComponent;

            public readonly RenderView RenderView;

            public LightComponentKey(LightComponent lightComponent, RenderView renderView)
            {
                LightComponent = lightComponent;
                RenderView = renderView;
            }

            public bool Equals(LightComponentKey other)
            {
                return LightComponent == other.LightComponent && RenderView == other.RenderView;
            }

            public override int GetHashCode()
            {
                return LightComponent.GetHashCode() ^ (397 * RenderView.GetHashCode());
            }
        }
    }
}