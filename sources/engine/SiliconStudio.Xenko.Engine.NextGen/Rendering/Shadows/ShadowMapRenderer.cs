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

namespace SiliconStudio.Xenko.Rendering.Shadows.NextGen
{
    /// <summary>
    /// Handles rendering of shadow map casters.
    /// </summary>
    public class ShadowMapRenderer
    {
        // TODO: Extract a common interface and implem for shadow renderer (not only shadow maps)

        public NextGenRenderSystem RenderSystem { get; set; }

        private FastListStruct<ShadowMapAtlasTexture> atlases;

        private PoolListStruct<LightShadowMapTexture> shadowMapTextures;

        private readonly int MaximumTextureSize = (int)(ReferenceShadowSize * ComputeSizeFactor(LightShadowMapSize.XLarge) * 2.0f);

        private const float ReferenceShadowSize = 1024;

        internal static readonly ParameterKey<ShadowMapReceiverInfo[]> Receivers = ParameterKeys.New(new ShadowMapReceiverInfo[1]);
        internal static readonly ParameterKey<ShadowMapReceiverVsmInfo[]> ReceiversVsm = ParameterKeys.New(new ShadowMapReceiverVsmInfo[1]);
        internal static readonly ParameterKey<ShadowMapCascadeLevel[]> LevelReceivers = ParameterKeys.New(new ShadowMapCascadeLevel[1]);
        internal static readonly ParameterKey<int> ShadowMapLightCount = ParameterKeys.New(0);

        /// <summary>
        /// The shadow map caster extension a discard extension
        /// </summary>
        private static readonly ShaderMixinGeneratorSource ShadowMapCasterExtension = new ShaderMixinGeneratorSource("ShadowMapCaster") { Discard = true };

        // rectangles to blur for each shadow map
        private HashSet<LightShadowMapTexture> shadowMapTexturesToBlur = new HashSet<LightShadowMapTexture>();

        private readonly ModelComponentRenderer shadowModelComponentRenderer;

        private readonly ParameterCollection shadowCasterParameters;

        public readonly Dictionary<LightComponent, LightShadowMapTexture> LightComponentsWithShadows;

        private List<LightComponent> visibleLights;

        public ShadowMapRenderer(NextGenRenderSystem renderSystem)
        {
            RenderSystem = renderSystem;
            atlases = new FastListStruct<ShadowMapAtlasTexture>(16);
            shadowMapTextures = new PoolListStruct<LightShadowMapTexture>(16, CreateLightShadowMapTexture);
            LightComponentsWithShadows = new Dictionary<LightComponent, LightShadowMapTexture>(16);

            Renderers = new Dictionary<Type, ILightShadowMapRenderer>();

            ShadowCamera = new CameraComponent { UseCustomViewMatrix = true, UseCustomProjectionMatrix = true };

            shadowCasterParameters = new ParameterCollection();
            shadowCasterParameters.Set(XenkoEffectBaseKeys.ExtensionPostVertexStageShader, ShadowMapCasterExtension);
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

        public void Extract(RenderContext context, List<LightComponent> visibleLights)
        {
            this.visibleLights = visibleLights;

            // We must be running inside the context of 
            var sceneInstance = SceneInstance.GetCurrent(context);
            if (sceneInstance == null)
            {
                throw new InvalidOperationException("ShadowMapRenderer expects to be used inside the context of a SceneInstance.Draw()");
            }

            // Gets the current camera
            Camera = context.GetCurrentCamera();
            if (Camera == null)
            {
                return;
            }

            // Clear currently associated shadows
            shadowMapTextures.Clear();
            LightComponentsWithShadows.Clear();

            // Collect all required shadow maps
            CollectShadowMaps();

            // No shadow maps to render
            if (shadowMapTextures.Count == 0)
            {
                return;
            }

            // Assign rectangles to shadow maps
            AssignRectangles();

            // Reset the state of renderers
            foreach (var rendererKeyPairs in Renderers)
            {
                var renderer = rendererKeyPairs.Value;
                renderer.Reset();
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

                var texture = Texture.New2D(RenderSystem.GraphicsDevice, MaximumTextureSize, MaximumTextureSize, 1, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
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
            currentAtlas.ClearRenderTarget(RenderSystem.RenderContextOld);
            lightShadowMapTexture.TextureId = (byte)currentAtlas.Id;
            lightShadowMapTexture.Atlas = currentAtlas;
        }

        private void CollectShadowMaps()
        {
            foreach (var lightComponent in visibleLights)
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
                var size = light.ComputeScreenCoverage(RenderSystem.RenderContextOld, position, direction);

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
                LightComponentsWithShadows.Add(lightComponent, shadowMapTexture);
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
    }
}