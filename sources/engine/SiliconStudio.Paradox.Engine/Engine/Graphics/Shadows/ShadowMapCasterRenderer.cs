// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Paradox.Effects.Lights;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.Engine.Graphics.Shadows;
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
        private FastListStruct<ShadowMapTexture> shadowMaps;

        private readonly int MaximumTextureSize = (int)(MaximumShadowSize * ComputeSizeFactor(LightShadowImportance.High, LightShadowMapSize.Large) * 2.0f);

        private const float MaximumShadowSize = 1024;

        private const float VsmBlurSize = 4.0f;

        /// <summary>
        /// Base points for frustum corners.
        /// </summary>
        private static readonly Vector3[] FrustumBasePoints =
            {
                new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f),
                new Vector3(-1.0f,-1.0f, 1.0f), new Vector3(1.0f,-1.0f, 1.0f), new Vector3(-1.0f,1.0f, 1.0f), new Vector3(1.0f,1.0f, 1.0f),
            };

        /// <summary>
        /// The various UP vectors to try.
        /// </summary>
        private static readonly Vector3[] VectorUps = { Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX };

        internal static readonly ParameterKey<ShadowMapReceiverInfo[]> Receivers = ParameterKeys.New(new ShadowMapReceiverInfo[1]);
        internal static readonly ParameterKey<ShadowMapReceiverVsmInfo[]> ReceiversVsm = ParameterKeys.New(new ShadowMapReceiverVsmInfo[1]);
        internal static readonly ParameterKey<ShadowMapCascadeLevel[]> LevelReceivers = ParameterKeys.New(new ShadowMapCascadeLevel[1]);
        internal static readonly ParameterKey<int> ShadowMapLightCount = ParameterKeys.New(0);
        
        // Storage for temporary variables
        private Vector3[] points = new Vector3[8];
        private Vector3[] directions = new Vector3[4];

        // rectangles to blur for each shadow map
        private HashSet<ShadowMapTexture> shadowMapTexturesToBlur = new HashSet<ShadowMapTexture>();

        private readonly ParameterCollection blurParameters;

        private readonly SceneGraphicsCompositorLayers compositor;

        private readonly Entity cameraEntity;

        private readonly CameraComponent shadowCameraComponent;

        public ShadowMapCasterRenderer()
        {
            atlases = new FastListStruct<ShadowMapAtlasTexture>();
            shadowMaps = new FastListStruct<ShadowMapTexture>(16);

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
                                FilterComponentTypes = { typeof(CameraComponent), typeof(ModelComponent) }
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
            var sceneInstance = context.Tags.Get(Engine.SceneInstance.Current);
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

            if (!CollectShadowMaps(context)) 
                return;

            // Assign rectangles to shadow maps
            AssignRectangles(context);

            var graphicsDevice = context.GraphicsDevice;


            // Get View and Projection matrices
            var view = camera.ViewMatrix;
            var projection = camera.ProjectionMatrix;

            // Compute frustum-dependent variables (common for all shadow maps)
            Matrix inverseView, inverseProjection;
            Matrix.Invert(ref projection, out inverseProjection);
            Matrix.Invert(ref view, out inverseView);

            // Transform Frustum corners in View Space (8 points) - algorithm is valid only if the view matrix does not do any kind of scale/shear transformation
            for (int i = 0; i < 8; ++i)
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref inverseProjection, out points[i]);

            // Compute frustum edge directions
            for (int i = 0; i < 4; i++)
                directions[i] = Vector3.Normalize(points[i + 4] - points[i]);

            // Prepare and render shadow maps
            foreach (var shadowMap in shadowMaps)
            {
                // Compute shadow map infos
                //ComputeShadowMap(shadowMap, ref inverseView);

                //if (shadowMap.Filter == LightShadowMapFilterType.Variance)
                //    graphicsDevice.SetDepthAndRenderTarget(shadowMap.Texture.ShadowMapDepthTexture, shadowMap.Texture.ShadowMapTargetTexture);
                //else
                //    graphicsDevice.SetDepthTarget(shadowMap.Texture.ShadowMapDepthTexture);

                // set layers
                context.Parameters.Set(RenderingParameters.ActiveRenderLayer, shadowMap.Layers);

                // Render each cascade
                for (int i = 0; i < shadowMap.CascadeCount; ++i)
                {
                    var cascade = shadowMap.Cascades[i];

                    // Override with current shadow map parameters
                    graphicsDevice.Parameters.Set(ShadowMapKeys.DistanceMax, shadowMap.LightType == LightType.Directional ? shadowMap.ShadowFarDistance : shadowMap.ShadowFarDistance - shadowMap.ShadowNearDistance);
                    graphicsDevice.Parameters.Set(LightKeys.LightDirection, shadowMap.LightDirectionNormalized);
                    graphicsDevice.Parameters.Set(ShadowMapKeys.LightOffset, cascade.CascadeLevels.Offset);

                    // We computed ViewProjection, so let's use View = Identity & Projection = ViewProjection
                    // (ideally we should override ViewProjection dynamic)
                    graphicsDevice.Parameters.Set(TransformationKeys.View, Matrix.Identity);
                    graphicsDevice.Parameters.Set(TransformationKeys.Projection, cascade.ViewProjCaster);

                    // Prepare viewport
                    var cascadeTextureCoord = cascade.CascadeTextureCoords;
                    var viewPortCoord = new Vector4(
                        cascadeTextureCoord.X * shadowMap.Texture.ShadowMapDepthTexture.ViewWidth,
                        cascadeTextureCoord.Y * shadowMap.Texture.ShadowMapDepthTexture.ViewHeight,
                        cascadeTextureCoord.Z * shadowMap.Texture.ShadowMapDepthTexture.ViewWidth,
                        cascadeTextureCoord.W * shadowMap.Texture.ShadowMapDepthTexture.ViewHeight);

                    // Set viewport
                    graphicsDevice.SetViewport(new Viewport((int)viewPortCoord.X, (int)viewPortCoord.Y, (int)(viewPortCoord.Z - viewPortCoord.X), (int)(viewPortCoord.W - viewPortCoord.Y)));

                    if (shadowMap.Filter == LightShadowMapFilterType.Variance)
                        shadowMapTexturesToBlur.Add(shadowMap.Texture);
                        
                    graphicsDevice.Parameters.Set(ShadowMapParameters.FilterType, shadowMap.Filter);
                    base.OnRendering(context);
                }

                // reset layers
                context.Parameters.Reset(RenderingParameters.ActiveRenderLayer);
            }

            // Reset parameters
            graphicsDevice.Parameters.Reset(ShadowMapKeys.DistanceMax);
            graphicsDevice.Parameters.Reset(LightKeys.LightDirection);
            graphicsDevice.Parameters.Reset(ShadowMapKeys.LightOffset);
            graphicsDevice.Parameters.Reset(TransformationKeys.View);
            graphicsDevice.Parameters.Reset(TransformationKeys.Projection);
            if (hasFilter)
                graphicsDevice.Parameters.Set(ShadowMapParameters.FilterType, filterBackup);
            else
                graphicsDevice.Parameters.Reset(ShadowMapParameters.FilterType);

            foreach (var shadowMap in shadowMapTexturesToBlur)
            {
                graphicsDevice.SetDepthStencilState(graphicsDevice.DepthStencilStates.None);
                graphicsDevice.SetRasterizerState(graphicsDevice.RasterizerStates.CullNone);

                // TODO: use next post effect instead
                graphicsDevice.SetDepthAndRenderTarget(shadowMap.ShadowMapDepthTexture, shadowMap.IntermediateBlurTexture);
                blurParameters.Set(TexturingKeys.Texture0, shadowMap.ShadowMapTargetTexture);
                blurParameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);
                graphicsDevice.DrawQuad(vsmHorizontalBlur, blurParameters);

                // TODO: use next post effect instead
                graphicsDevice.SetDepthAndRenderTarget(shadowMap.ShadowMapDepthTexture, shadowMap.ShadowMapTargetTexture);
                blurParameters.Set(TexturingKeys.Texture0, shadowMap.IntermediateBlurTexture);
                blurParameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);
                graphicsDevice.DrawQuad(vsmVerticalBlur, blurParameters);
            }

            shadowMapTexturesToBlur.Clear();
        }

        private void AssignRectangles(RenderContext context)
        {
            // Clear atlases
            for (int i = 0; i < atlases.Count; i++)
            {
                atlases[i].Clear();
            }

            // Assign rectangles for shadowmaps
            for (int i = 0; i < shadowMaps.Count; i++)
            {
                AssignRectangles(context, ref shadowMaps.Items[i]);
            }
        }

        private void AssignRectangles(RenderContext context, ref ShadowMapTexture shadowMapTexture)
        {
            // TODO: This is not good to have to detect the light type here
            shadowMapTexture.CascadeCount = shadowMapTexture.Light is LightDirectional ? (int)shadowMapTexture.Shadow.CascadeCount : 1;

            var size = shadowMapTexture.Size;

            // Try to fit the shadow map into an existing atlas
            for (int i = 0; i < atlases.Count; i++)
            {
                var atlas = atlases[i];
                if (atlas.FilterType == shadowMapTexture.FilterType && atlas.TryInsert(size, size, shadowMapTexture.CascadeCount))
                {
                    AssignRectangles(context, ref shadowMapTexture, atlas);
                    return;
                }
            }
            
            // Allocate a new atlas texture
            var texture = Texture.New2D(context.GraphicsDevice, MaximumTextureSize, MaximumTextureSize, 1, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            var newAtlas = new ShadowMapAtlasTexture(texture) { FilterType = shadowMapTexture.FilterType };
            atlases.Add(newAtlas);
        }

        private void AssignRectangles(RenderContext context, ref ShadowMapTexture shadowMapTexture, ShadowMapAtlasTexture atlas)
        {
            if (!atlas.IsRenderTargetCleared)
            {
                atlas.ClearRenderTarget(context);
            }

            var size = shadowMapTexture.Size;
            for (int i = 0; i < shadowMapTexture.CascadeCount; i++)
            {
                var rect = Rectangle.Empty;
                atlas.Insert(size, size, ref rect);
                shadowMapTexture.SetRectangle(i, rect);
            }
        }

        private bool CollectShadowMaps(RenderContext context)
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
                    var size = light.ComputeScreenCoverage(context, position, direction);

                    // Converts the importance into a shadow size factor
                    var sizeFactor = ComputeSizeFactor(light.ShadowImportance, shadowMap.Size);
                    
                    

                    // Compute the size of the final shadow map
                    // TODO: Handle GraphicsProfile
                    var shadowMapSize = (int)Math.Min(MaximumShadowSize * sizeFactor, MathUtil.NextPowerOfTwo(size * sizeFactor));

                    if (shadowMapSize <= 0) // TODO: Validate < 0 earlier in the setters
                    {
                        continue;
                    }

                    shadowMaps.Add(new ShadowMapTexture(lightComponent, light, shadowMap, shadowMapSize));
                }
            }

            // No shadow maps to render
            if (shadowMaps.Count == 0)
            {
                return false;
            }

            return true;
        }

        private void ComputeShadowMap(ShadowMapTexture shadowMap, ref Matrix inverseView)
        {
            float shadowDistribute = 1.0f / shadowMap.CascadeCount;
            float znear = shadowMap.ShadowNearDistance;
            float zfar = shadowMap.ShadowFarDistance;

            var boudingBoxVectors = new Vector3[8];
            var direction = shadowMap.LightComponent.Direction;

            // Fake value
            // It will be setup by next loop
            Vector3 side = Vector3.UnitX;
            Vector3 up = Vector3.UnitX;

            // Select best Up vector
            // TODO: User preference?
            foreach (var vectorUp in VectorUps)
            {
                if (Vector3.Dot(direction, vectorUp) < (1.0 - 0.0001))
                {
                    side = Vector3.Normalize(Vector3.Cross(vectorUp, direction));
                    up = Vector3.Normalize(Vector3.Cross(direction, side));
                    break;
                }
            }

            // Prepare cascade list (allocate it if not done yet)
            var cascades = shadowMap.Cascades;
            if (cascades == null)
                cascades = shadowMap.Cascades = new ShadowMapCascadeInfo[shadowMap.CascadeCount];

            bool stableCascades = false;

            for (int cascadeLevel = 0; cascadeLevel < shadowMap.CascadeCount; ++cascadeLevel)
            {
                // Compute caster view and projection matrices
                var shadowMapView = Matrix.Zero;
                var shadowMapProjection = Matrix.Zero;
                if (shadowMap.LightType == LightType.Directional)
                {
                    // Compute cascade split (between znear and zfar)
                    float k0 = (float)(cascadeLevel + 0) / shadowMap.CascadeCount;
                    float k1 = (float)(cascadeLevel + 1) / shadowMap.CascadeCount;
                    float min = (float)(znear * Math.Pow(zfar / znear, k0)) * (1.0f - shadowDistribute) + (znear + (zfar - znear) * k0) * shadowDistribute;
                    float max = (float)(znear * Math.Pow(zfar / znear, k1)) * (1.0f - shadowDistribute) + (znear + (zfar - znear) * k1) * shadowDistribute;

                    // Compute frustum corners
                    for (int j = 0; j < 4; j++)
                    {
                        boudingBoxVectors[j * 2 + 0] = points[j] + directions[j] * min;
                        boudingBoxVectors[j * 2 + 1] = points[j] + directions[j] * max;
                    }
                    var boundingBox = BoundingBox.FromPoints(boudingBoxVectors);

                    // Compute bounding box center & radius
                    // Note: boundingBox is computed in view space so the computation of the radius is only correct when the view matrix does not do any kind of scale/shear transformation
                    var radius = (boundingBox.Maximum - boundingBox.Minimum).Length() * 0.5f;
                    var target = Vector3.TransformCoordinate(boundingBox.Center, inverseView);

                    // Snap camera to texel units (so that shadow doesn't jitter when light doesn't change direction but camera is moving)
                    var shadowMapHalfSize = shadowMap.ShadowMapSize * 0.5f;
                    float x = (float)Math.Ceiling(Vector3.Dot(target, up) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float z = Vector3.Dot(target, direction);
                    //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                    target = up * x + side * y + direction * z;

                    // Compute caster view and projection matrices
                    shadowMapView = Matrix.LookAtRH(target - direction * zfar * 0.5f, target + direction * zfar * 0.5f, up); // View;
                    shadowMapProjection = Matrix.OrthoOffCenterRH(-radius, radius, -radius, radius, znear, zfar); // Projection
                    // near and far are not correctly set but the shader will rewrite the z value
                    // on the hand, the offset is correct

                    // Compute offset
                    Matrix shadowVInverse;
                    Matrix.Invert(ref shadowMapView, out shadowVInverse);
                    cascades[cascadeLevel].CascadeLevels.Offset = new Vector3(shadowVInverse.M41, shadowVInverse.M42, shadowVInverse.M43);
                }
                else if (shadowMap.LightType == LightType.Spot)
                {
                    shadowMapView = Matrix.LookAtRH(shadowMap.LightPosition, shadowMap.LightPosition + shadowMap.LightDirection, up);
                    shadowMapProjection = Matrix.PerspectiveFovRH(shadowMap.Fov, 1, znear, zfar);

                    // Set offset
                    cascades[cascadeLevel].CascadeLevels.Offset = shadowMap.LightPosition + znear * shadowMap.LightDirectionNormalized;
                }

                // Allocate shadow map area
                var shadowMapRectangle = new Rectangle();
                if (!shadowMap.Texture.GuillotinePacker.Insert(shadowMap.ShadowMapSize, shadowMap.ShadowMapSize, ref shadowMapRectangle))
                    throw new InvalidOperationException("Not enough space to allocate all shadow maps.");

                var cascadeTextureCoords = new Vector4(
                    (float)shadowMapRectangle.Left / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewWidth,
                    (float)shadowMapRectangle.Top / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewHeight,
                    (float)shadowMapRectangle.Right / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewWidth,
                    (float)shadowMapRectangle.Bottom / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewHeight);

                // Copy texture coords without border
                cascades[cascadeLevel].CascadeTextureCoords = cascadeTextureCoords;

                // Add border (avoid using edges due to bilinear filtering and blur)
                var borderSizeU = VsmBlurSize / shadowMap.Texture.ShadowMapDepthTexture.ViewWidth;
                var borderSizeV = VsmBlurSize / shadowMap.Texture.ShadowMapDepthTexture.ViewHeight;
                cascadeTextureCoords.X += borderSizeU;
                cascadeTextureCoords.Y += borderSizeV;
                cascadeTextureCoords.Z -= borderSizeU;
                cascadeTextureCoords.W -= borderSizeV;

                float leftX = (float)shadowMap.ShadowMapSize / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewWidth * 0.5f;
                float leftY = (float)shadowMap.ShadowMapSize / (float)shadowMap.Texture.ShadowMapDepthTexture.ViewHeight * 0.5f;
                float centerX = 0.5f * (cascadeTextureCoords.X + cascadeTextureCoords.Z);
                float centerY = 0.5f * (cascadeTextureCoords.Y + cascadeTextureCoords.W);

                // Compute caster view proj matrix
                Matrix.Multiply(ref shadowMapView, ref shadowMapProjection, out cascades[cascadeLevel].ViewProjCaster);

                // Compute receiver view proj matrix
                // TODO: Optimize adjustment matrix computation
                Matrix adjustmentMatrix = Matrix.Scaling(leftX, -leftY, 0.5f) * Matrix.Translation(centerX, centerY, 0.5f);
                Matrix.Multiply(ref cascades[cascadeLevel].ViewProjCaster, ref adjustmentMatrix, out cascades[cascadeLevel].CascadeLevels.ViewProjReceiver);

                // Copy texture coords with border
                cascades[cascadeLevel].CascadeLevels.CascadeTextureCoordsBorder = cascadeTextureCoords;
            }
        }

        private static float ComputeSizeFactor(LightShadowImportance importance, LightShadowMapSize shadowMapSize)
        {
            // Calculate a basic factor from the importance of this shadow map
            var factor = importance == LightShadowImportance.High ? 2.0f : importance == LightShadowImportance.Medium ? 1.0f : 0.5f;

            // Then reduce the size based on the shadow map size
            factor *= (float)Math.Pow(2.0f, (int)shadowMapSize - 2.0f);
            return factor;
        }

        private struct ShadowMapTexture
        {
            public ShadowMapTexture(LightComponent lightComponent, IDirectLight light, LightShadowMap shadowMap, int size)
                : this()
            {
                if (lightComponent == null) throw new ArgumentNullException("lightComponent");
                if (light == null) throw new ArgumentNullException("light");
                if (shadowMap == null) throw new ArgumentNullException("shadowMap");
                LightComponent = lightComponent;
                Light = light;
                Shadow = shadowMap;
                Size = size;
                FilterType = Shadow.Filter == null || !Shadow.Filter.RequiresCustomBuffer() ? null : Shadow.Filter.GetType();
            }

            public readonly LightComponent LightComponent;

            public readonly IDirectLight Light;

            public readonly LightShadowMap Shadow;

            public readonly Type FilterType;

            public readonly int Size;

            public int CascadeCount;

            public Rectangle GetRectangle(int i)
            {
                if (i < 0 || i > CascadeCount)
                {
                    throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
                }
                unsafe
                {
                    fixed (void* ptr = &Rectangle0)
                    {
                        return ((Rectangle*)ptr)[i];
                    }  
                }
            }

            public void SetRectangle(int i, Rectangle value)
            {
                if (i < 0 || i > CascadeCount)
                {
                    throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
                }
                unsafe
                {
                    fixed (void* ptr = &Rectangle0)
                    {
                        ((Rectangle*)ptr)[i] = value;
                    }
                }
            }

            private Rectangle Rectangle0;

            private Rectangle Rectangle1;

            private Rectangle Rectangle2;

            private Rectangle Rectangle3;

            public ShadowMapAtlasTexture Atlas;
        }

        private class ShadowMapAtlasTexture : GuillotinePacker
        {
            public ShadowMapAtlasTexture(Texture texture)
            {
                if (texture == null) throw new ArgumentNullException("texture");
                Texture = texture;
                Clear(Texture.Width, Texture.Height);

                RenderFrame = RenderFrame.FromTexture(null, texture);
            }

            public Type FilterType;

            public readonly Texture Texture;

            public readonly RenderFrame RenderFrame;

            public bool IsRenderTargetCleared { get; private set; }

            public override void Clear()
            {
                base.Clear();
                IsRenderTargetCleared = false;
            }

            public void ClearRenderTarget(RenderContext context)
            {
                context.GraphicsDevice.Clear(Texture, DepthStencilClearOptions.DepthBuffer);
                IsRenderTargetCleared = true;
            }
        }
    }
}