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
        private FastListStruct<ShadowMapTexture> shadowMaps;

        private readonly int MaximumTextureSize = (int)(MaximumShadowSize * ComputeSizeFactor(LightShadowImportance.High, LightShadowMapSize.Large) * 2.0f);

        private const float MaximumShadowSize = 1024;

        private const float VsmBlurSize = 4.0f;

        internal static readonly ParameterKey<ShadowMapReceiverInfo[]> Receivers = ParameterKeys.New(new ShadowMapReceiverInfo[1]);
        internal static readonly ParameterKey<ShadowMapReceiverVsmInfo[]> ReceiversVsm = ParameterKeys.New(new ShadowMapReceiverVsmInfo[1]);
        internal static readonly ParameterKey<ShadowMapCascadeLevel[]> LevelReceivers = ParameterKeys.New(new ShadowMapCascadeLevel[1]);
        internal static readonly ParameterKey<int> ShadowMapLightCount = ParameterKeys.New(0);
        

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

            // Collect all required shadow maps
            if (!CollectShadowMaps(context)) 
                return;

            // Assign rectangles to shadow maps
            AssignRectangles(context);

            var graphicsDevice = context.GraphicsDevice;

            // Get View and Projection matrices
            var shadowMapContext = new ShadowMapCasterContext(camera);

            // Prepare and render shadow maps
            for (int i = 0; i < shadowMaps.Count; i++)
            {
                shadowMaps[i].Renderer.Render(shadowMapContext, ref shadowMaps.Items[i]);
            }
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
            // Make sure the atlas cleared (will be clear just once)
            atlas.ClearRenderTarget(context);

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

        private static float ComputeSizeFactor(LightShadowImportance importance, LightShadowMapSize shadowMapSize)
        {
            // Calculate a basic factor from the importance of this shadow map
            var factor = importance == LightShadowImportance.High ? 2.0f : importance == LightShadowImportance.Medium ? 1.0f : 0.5f;

            // Then reduce the size based on the shadow map size
            factor *= (float)Math.Pow(2.0f, (int)shadowMapSize - 2.0f);
            return factor;
        }
    }

    public class ShadowMapCasterContext
    {
        /// <summary>
        /// Base points for frustum corners.
        /// </summary>
        private static readonly Vector3[] FrustumBasePoints =
        {
            new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f),
            new Vector3(-1.0f,-1.0f, 1.0f), new Vector3(1.0f,-1.0f, 1.0f), new Vector3(-1.0f,1.0f, 1.0f), new Vector3(1.0f,1.0f, 1.0f),
        };

        public ShadowMapCasterContext(CameraComponent camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            Camera = camera;

            FrustumCorner = new Vector3[8];
            FrustumDirection = new Vector3[4];

            Initialize(camera);
        }

        /// <summary>
        /// The frustum corner positions in world space
        /// </summary>
        public readonly Vector3[] FrustumCorner;

        /// <summary>
        /// The frustum direction in world space
        /// </summary>
        public readonly Vector3[] FrustumDirection;

        /// <summary>
        /// The view to world.
        /// </summary>
        public Matrix ViewToWorld;

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        public CameraComponent Camera { get; private set; }

        public void Initialize(CameraComponent camera)
        {
            Camera = camera;

            var projection = camera.ProjectionMatrix;
            var view = camera.ViewMatrix;

            // Compute frustum-dependent variables (common for all shadow maps)
            Matrix projectionToViewSpace;
            Matrix.Invert(ref projection, out projectionToViewSpace);
            Matrix.Invert(ref view, out ViewToWorld);

            Matrix screenToWorld;
            Matrix.Multiply(ref projectionToViewSpace, ref ViewToWorld, out screenToWorld);

            // Transform Frustum corners in World Space (8 points) - algorithm is valid only if the view matrix does not do any kind of scale/shear transformation
            for (int i = 0; i < 8; ++i)
            {
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref screenToWorld, out FrustumCorner[i]);
            }

            // Compute frustum edge directions
            for (int i = 0; i < 4; i++)
            {
                FrustumDirection[i] = Vector3.Normalize(FrustumCorner[i + 4] - FrustumCorner[i]);
            }
        }
    }

    public interface ILightShadowMapRenderer
    {
        void Render(ShadowMapCasterContext shadowContext, ref ShadowMapTexture shadowMap);
    }

    public class LightDirectionalShadowMapRenderer : ILightShadowMapRenderer
    {
        /// <summary>
        /// The various UP vectors to try.
        /// </summary>
        private static readonly Vector3[] VectorUps = { Vector3.UnitZ, Vector3.UnitY, Vector3.UnitX };

        public LightDirectionalShadowMapRenderer()
        {
            CascadeCasterMatrix = new Matrix[4];
            CascadeToUVMatrix = new Matrix[4];
            CascadeSplitRatios = new float[4];
            CascadeSplits = new float[4];
            CascadeOffsets = new Vector3[4];
            CascadeScales = new Vector3[4];
            CascadeRectangleUVs = new Vector4[4];
            CascadeFrustumCorners = new Vector3[8];
        }

        public readonly Matrix[] CascadeCasterMatrix;

        public readonly Matrix[] CascadeToUVMatrix;

        public readonly float[] CascadeSplitRatios;

        public readonly float[] CascadeSplits;

        public readonly Vector3[] CascadeOffsets;

        public readonly Vector3[] CascadeScales;

        public readonly Vector4[] CascadeRectangleUVs;

        private Vector3[] CascadeFrustumCorners;

        private void ComputeCascadeSplits(ShadowMapCasterContext shadowContext, ref ShadowMapTexture shadowMap)
        {
            var shadow = shadowMap.Shadow;

            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var minDistance = shadow.MinDistance;
            var maxDistance = shadow.MinDistance;

            if (shadow.SplitMode == LightShadowMapSplitMode.Logarithmic || shadow.SplitMode == LightShadowMapSplitMode.PSSM)
            {
                var nearClip = shadowContext.Camera.NearClipPlane;
                var farClip = shadowContext.Camera.FarClipPlane;
                var rangeClip = farClip - nearClip;

                var minZ = nearClip + minDistance * rangeClip;
                var maxZ = nearClip + maxDistance * rangeClip;

                var range = maxZ - minZ;
                var ratio = maxZ / minZ;
                var logRatio = shadow.SplitMode == LightShadowMapSplitMode.Logarithmic ? 1.0f : 0.0f;

                for (int cascadeLevel = 0; cascadeLevel < shadowMap.CascadeCount; ++cascadeLevel)
                {
                    // Compute cascade split (between znear and zfar)
                    float distrib = (float)(cascadeLevel + 1) / shadowMap.CascadeCount;
                    float logZ = (float)(minZ * Math.Pow(ratio, distrib));
                    float uniformZ = minZ + range * distrib;
                    float distance = MathUtil.Lerp(uniformZ, logZ, logRatio);
                    CascadeSplitRatios[cascadeLevel] = (distance - nearClip) / rangeClip;  // Normalize cascade splits to [0,1]
                }
            }
            else
            {
                CascadeSplitRatios[0] = minDistance + shadow.SplitDistance0 * maxDistance;
                CascadeSplitRatios[1] = minDistance + shadow.SplitDistance1 * maxDistance;
                CascadeSplitRatios[2] = minDistance + shadow.SplitDistance2 * maxDistance;
                CascadeSplitRatios[3] = minDistance + shadow.SplitDistance3 * maxDistance;
            }
        }

        public void Render(ShadowMapCasterContext shadowContext, ref ShadowMapTexture shadowMap)
        {
            // Computes the cascade splits
            ComputeCascadeSplits(shadowContext, ref shadowMap);
            var direction = shadowMap.LightComponent.Direction;

            // Fake value
            // It will be setup by next loop
            Vector3 side = Vector3.UnitX;
            Vector3 upDirection = Vector3.UnitX;

            // Select best Up vector
            // TODO: User preference?
            foreach (var vectorUp in VectorUps)
            {
                if (Vector3.Dot(direction, vectorUp) < (1.0 - 0.0001))
                {
                    side = Vector3.Normalize(Vector3.Cross(vectorUp, direction));
                    upDirection = Vector3.Normalize(Vector3.Cross(direction, side));
                    break;
                }
            }

            var shadow = shadowMap.Shadow;
            // TODO: Min and Max distance can be auto-computed from readback from Z buffer
            var minDistance = shadow.MinDistance;
            var maxDistance = shadow.MinDistance;
            var camera = shadowContext.Camera;

            for (int cascadeLevel = 0; cascadeLevel < shadowMap.CascadeCount; ++cascadeLevel)
            {
                // Compute caster view and projection matrices
                var shadowMapView = Matrix.Zero;
                var shadowMapProjection = Matrix.Zero;

                float max = minDistance;

                // Calculate frustum corners for this cascade
                for (int j = 0; j < 4; j++)
                {
                    var min = max;
                    max = CascadeSplitRatios[cascadeLevel];
                    CascadeFrustumCorners[j * 2 + 0] = shadowContext.FrustumCorner[j] + shadowContext.FrustumDirection[j] * min;
                    CascadeFrustumCorners[j * 2 + 1] = shadowContext.FrustumCorner[j] + shadowContext.FrustumDirection[j] * max;
                }
                var cascadeBounds = BoundingBox.FromPoints(CascadeFrustumCorners);

                var orthoMin = Vector3.Zero;
                var orthoMax = Vector3.Zero;

                var target = cascadeBounds.Center;

                if (shadow.Stabilized)
                {
                    // Compute bounding box center & radius
                    // Note: boundingBox is computed in view space so the computation of the radius is only correct when the view matrix does not do any kind of scale/shear transformation
                    var radius = (cascadeBounds.Maximum - cascadeBounds.Minimum).Length() * 0.5f;

                    orthoMax = new Vector3(radius, radius, radius);
                    orthoMin = -orthoMax;

                    // Make sure we are using the same direction when stabilizing
                    upDirection = shadowContext.Camera.ViewMatrix.Right;

                    // Snap camera to texel units (so that shadow doesn't jitter when light doesn't change direction but camera is moving)
                    var shadowMapHalfSize = shadowMap.Size * 0.5f;
                    float x = (float)Math.Ceiling(Vector3.Dot(target, upDirection) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float y = (float)Math.Ceiling(Vector3.Dot(target, side) * shadowMapHalfSize / radius) * radius / shadowMapHalfSize;
                    float z = Vector3.Dot(target, direction);
                    //target = up * x + side * y + direction * R32G32B32_Float.Dot(target, direction);
                    target = upDirection * x + side * y + direction * z;
                }
                else
                {
                    var lightViewMatrix = Matrix.LookAtLH(cascadeBounds.Center - direction, cascadeBounds.Center, upDirection);
                    orthoMin = new Vector3(float.MaxValue);
                    orthoMax = new Vector3(-float.MaxValue);
                    for (int i = 0; i < CascadeFrustumCorners.Length; i++)
                    {
                        Vector3 cornerViewSpace;
                        Vector3.TransformCoordinate(ref CascadeFrustumCorners[i], ref lightViewMatrix, out cornerViewSpace);

                        orthoMin = Vector3.Min(orthoMin, cornerViewSpace);
                        orthoMax = Vector3.Min(orthoMax, cornerViewSpace);
                    }

                    // TODO: Adjust orthoSize by taking into account filtering size
                }

                // Compute caster view and projection matrices
                shadowMapView = Matrix.LookAtRH(target - direction * orthoMin.Z, target, upDirection); // View;
                shadowMapProjection = Matrix.OrthoOffCenterRH(orthoMin.X, orthoMax.X, orthoMin.Y, orthoMax.Y, 0.0f, orthoMax.Z - orthoMin.Z); // Projection

                // Calculate View Proj matrix from World space to Cascade space
                Matrix.Multiply(ref shadowMapView, ref shadowMapProjection, out CascadeCasterMatrix[cascadeLevel]);

                // Cascade splits in light space using depth
                CascadeSplits[cascadeLevel] = camera.NearClipPlane + CascadeSplitRatios[cascadeLevel] * (camera.FarClipPlane - camera.NearClipPlane);

                // Cascade offsets
                Matrix lightSpaceToWorld;
                Matrix.Invert(ref shadowMapView, out lightSpaceToWorld);
                CascadeOffsets[cascadeLevel] = lightSpaceToWorld.TranslationVector;

                var shadowMapRectangle = shadowMap.GetRectangle(cascadeLevel);

                var cascadeTextureCoords = new Vector4((float)shadowMapRectangle.Left / shadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Top / shadowMap.Atlas.Height,
                    (float)shadowMapRectangle.Right / shadowMap.Atlas.Width,
                    (float)shadowMapRectangle.Bottom / shadowMap.Atlas.Height);

                // Copy texture coords without border
                CascadeRectangleUVs[cascadeLevel] = cascadeTextureCoords;

                //// Add border (avoid using edges due to bilinear filtering and blur)
                //var borderSizeU = VsmBlurSize / shadowMap.Atlas.Width;
                //var borderSizeV = VsmBlurSize / shadowMap.Atlas.Height;
                //cascadeTextureCoords.X += borderSizeU;
                //cascadeTextureCoords.Y += borderSizeV;
                //cascadeTextureCoords.Z -= borderSizeU;
                //cascadeTextureCoords.W -= borderSizeV;

                float leftX = (float)shadowMap.Size / shadowMap.Atlas.Width * 0.5f;
                float leftY = (float)shadowMap.Size / shadowMap.Atlas.Height * 0.5f;
                float centerX = 0.5f * (cascadeTextureCoords.X + cascadeTextureCoords.Z);
                float centerY = 0.5f * (cascadeTextureCoords.Y + cascadeTextureCoords.W);

                // Compute receiver view proj matrix
                Matrix adjustmentMatrix = Matrix.Scaling(leftX, -leftY, 0.5f) * Matrix.Translation(centerX, centerY, 0.5f);
                Matrix.Multiply(ref CascadeCasterMatrix[cascadeLevel], ref adjustmentMatrix, out CascadeToUVMatrix[cascadeLevel]);

                //// Copy texture coords with border
                //cascades[cascadeLevel].CascadeLevels.CascadeTextureCoordsBorder = cascadeTextureCoords;
            }
        }
    }

    public struct ShadowMapTexture
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

        public int CascadeCount { get; set; }

        public ShadowMapAtlasTexture Atlas { get; internal set; }

        public ILightShadowMapRenderer Renderer;

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
    }



    public class ShadowMapAtlasTexture : GuillotinePacker
    {
        public ShadowMapAtlasTexture(Texture texture)
        {
            if (texture == null) throw new ArgumentNullException("texture");
            Texture = texture;
            Clear(Texture.Width, Texture.Height);
            Width = texture.Width;
            Height = texture.Height;

            RenderFrame = RenderFrame.FromTexture((Texture)null, texture);
        }

        public readonly int Width;

        public readonly int Height;

        public Type FilterType;

        public readonly Texture Texture;

        public readonly RenderFrame RenderFrame;

        private bool IsRenderTargetCleared;

        public override void Clear()
        {
            base.Clear();
            IsRenderTargetCleared = false;
        }

        public void ClearRenderTarget(RenderContext context)
        {
            if (!IsRenderTargetCleared)
            {
                context.GraphicsDevice.Clear(Texture, DepthStencilClearOptions.DepthBuffer);
                IsRenderTargetCleared = true;
            }
        }
    }
}