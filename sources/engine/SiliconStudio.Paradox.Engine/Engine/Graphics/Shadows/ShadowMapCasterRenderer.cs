// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    /// <summary>
    /// Handles rendering of shadow map casters.
    /// </summary>
    public class ShadowMapCasterRenderer : EntityComponentRendererCoreBase
    {
        private FastListStruct<ShadowMapAtlasTexture> atlases;
        private FastListStruct<LightShadowMapTexture> shadowMaps;

        private readonly int MaximumTextureSize = (int)(MaximumShadowSize * ComputeSizeFactor(LightShadowImportance.High, LightShadowMapSize.Large) * 2.0f);

        private const float MaximumShadowSize = 1024;

        private const float VsmBlurSize = 4.0f;

        internal static readonly ParameterKey<ShadowMapReceiverInfo[]> Receivers = ParameterKeys.New(new ShadowMapReceiverInfo[1]);
        internal static readonly ParameterKey<ShadowMapReceiverVsmInfo[]> ReceiversVsm = ParameterKeys.New(new ShadowMapReceiverVsmInfo[1]);
        internal static readonly ParameterKey<ShadowMapCascadeLevel[]> LevelReceivers = ParameterKeys.New(new ShadowMapCascadeLevel[1]);
        internal static readonly ParameterKey<int> ShadowMapLightCount = ParameterKeys.New(0);
        

        // rectangles to blur for each shadow map
        private HashSet<LightShadowMapTexture> shadowMapTexturesToBlur = new HashSet<LightShadowMapTexture>();

        private readonly SceneGraphicsCompositorLayers compositor;

        private readonly Entity cameraEntity;

        private readonly CameraComponent shadowCameraComponent;

        public ShadowMapCasterRenderer()
        {
            atlases = new FastListStruct<ShadowMapAtlasTexture>();
            shadowMaps = new FastListStruct<LightShadowMapTexture>(16);

            shadowCameraComponent = new CameraComponent();
            cameraEntity = new Entity() { shadowCameraComponent };

            // Declare the compositor used to render the current scene for the shadow mapping
            compositor = new SceneGraphicsCompositorLayers()
            {
                Cameras =
                {
                    shadowCameraComponent
                },
                Master =
                {
                    Renderers =
                    {
                        new SceneCameraRenderer()
                        {
                            Mode =
                            {
                                RenderComponentTypes = { typeof(CameraComponent), typeof(ModelComponent) }
                            }
                        }
                    }
                }
            };
        }

        public void Draw(RenderContext context)
        {
            PreDrawCoreInternal(context);
            DrawCore(context);
            PostDrawCoreInternal(context);
        }

        protected void DrawCore(RenderContext context)
        {
            // We must be running inside the context of 
            var sceneInstance = SceneInstance.GetCurrent(context);
            if (sceneInstance == null)
            {
                throw new InvalidOperationException("ShadowMapCasterRenderer expects to be used inside the context of a SceneInstance.Draw()");
            }
            
            // Gets the current camera
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return;
            }

            // Collect all required shadow maps
            if (!CollectShadowMaps()) 
                return;

            // Assign rectangles to shadow maps
            AssignRectangles();

            // Get View and Projection matrices
            var shadowMapContext = new ShadowMapCasterContext(camera);

            // Prepare and render shadow maps
            for (int i = 0; i < shadowMaps.Count; i++)
            {
                shadowMaps[i].Renderer.Render(shadowMapContext, ref shadowMaps.Items[i]);
            }
        }

        private void AssignRectangles()
        {
            // Clear atlases
            for (int i = 0; i < atlases.Count; i++)
            {
                atlases[i].Clear();
            }

            // Assign rectangles for shadowmaps
            for (int i = 0; i < shadowMaps.Count; i++)
            {
                AssignRectangles(ref shadowMaps.Items[i]);
            }
        }

        private void AssignRectangles(ref LightShadowMapTexture lightShadowMapTexture)
        {
            // TODO: This is not good to have to detect the light type here
            lightShadowMapTexture.CascadeCount = lightShadowMapTexture.Light is LightDirectional ? (int)lightShadowMapTexture.Shadow.CascadeCount : 1;

            var size = lightShadowMapTexture.Size;

            // Try to fit the shadow map into an existing atlas
            for (int i = 0; i < atlases.Count; i++)
            {
                var atlas = atlases[i];
                if (atlas.FilterType == lightShadowMapTexture.FilterType && atlas.TryInsert(size, size, lightShadowMapTexture.CascadeCount))
                {
                    AssignRectangles(ref lightShadowMapTexture, atlas);
                    return;
                }
            }
            
            // Allocate a new atlas texture
            var texture = Texture.New2D(Context.GraphicsDevice, MaximumTextureSize, MaximumTextureSize, 1, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            var newAtlas = new ShadowMapAtlasTexture(texture) { FilterType = lightShadowMapTexture.FilterType };
            atlases.Add(newAtlas);
        }

        private void AssignRectangles(ref LightShadowMapTexture lightShadowMapTexture, ShadowMapAtlasTexture atlas)
        {
            // Make sure the atlas cleared (will be clear just once)
            atlas.ClearRenderTarget(Context);

            var size = lightShadowMapTexture.Size;
            for (int i = 0; i < lightShadowMapTexture.CascadeCount; i++)
            {
                var rect = Rectangle.Empty;
                atlas.Insert(size, size, ref rect);
                lightShadowMapTexture.SetRectangle(i, rect);
            }
        }

        private bool CollectShadowMaps()
        {
            // Gets the LightProcessor
            var lightProcessor = SceneInstance.GetProcessor<LightProcessor>();
            if (lightProcessor == null)
                return false;

            // Prepare shadow map sizes
            shadowMaps.Clear();
            foreach (var activeLightsPerType in lightProcessor.ActiveLightsWithShadow)
            {
                var lightType = activeLightsPerType.Key;
                var lightComponents = activeLightsPerType.Value;

                foreach (var lightComponent in lightComponents)
                {
                    var light = (IDirectLight)lightComponent.Type;

                    // TODO: We support only ShadowMap in this renderer. Should we pre-organize this in the LightProcessor? (adding for example LightType => ShadowType => LightComponents)
                    var shadowMap = light.Shadow as LightShadowMap;
                    if (shadowMap == null)
                    {
                        continue;
                    }

                    var direction = lightComponent.Direction;
                    var position = lightComponent.Position;

                    // Compute the coverage of this light on the screen
                    var size = light.ComputeScreenCoverage(Context, position, direction);

                    // Converts the importance into a shadow size factor
                    var sizeFactor = ComputeSizeFactor(light.ShadowImportance, shadowMap.Size);
                    
                    // Compute the size of the final shadow map
                    // TODO: Handle GraphicsProfile
                    var shadowMapSize = (int)Math.Min(MaximumShadowSize * sizeFactor, MathUtil.NextPowerOfTwo(size * sizeFactor));

                    if (shadowMapSize <= 0) // TODO: Validate < 0 earlier in the setters
                    {
                        continue;
                    }

                    shadowMaps.Add(new LightShadowMapTexture(lightComponent, light, shadowMap, shadowMapSize));
                }
            }

            // No shadow maps to render
            if (shadowMaps.Count == 0)
            {
                return false;
            }

            return true;
        }

        private static float ComputeSizeFactor(LightShadowImportance importance, LightShadowMapSize shadowMapSize)
        {
            // Calculate a basic factor from the importance of this shadow map
            var factor = importance == LightShadowImportance.High ? 2.0f : importance == LightShadowImportance.Medium ? 1.0f : 0.5f;

            // Then reduce the size based on the shadow map size
            factor *= (float)Math.Pow(2.0f, (int)shadowMapSize - 2.0f);
            return factor;
        }
    }
}