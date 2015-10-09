// Copyright (c) 2011 ReShader - Alexandre Mutel

using System;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Yebis;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class YebisPlugin : RenderPassPlugin, IRenderPassPluginSource, IRenderPassPluginTarget
    {
        private Manager yebis;

        public bool AntiAlias;

        public ToneMap ToneMap;

        public Glare Glare;

        public Lens Lens;

        public DepthOfField DepthOfField;

        public ColorCorrection ColorCorrection;

        public HeatShimmer HeatShimmer;

        public LightShaft LightShaft;

        /// <summary>
        /// Initializes a new instance of the <see cref="YebisPlugin"/> class.
        /// </summary>
        public YebisPlugin() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YebisPlugin"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public YebisPlugin(string name) : base(name)
        {
            yebis = new Manager();
            ToneMap = yebis.Config.ToneMap;
            Glare = yebis.Config.Glare;
            ColorCorrection = yebis.Config.ColorCorrection;
            Lens = yebis.Config.Lens;
            DepthOfField = yebis.Config.DepthOfField;
            HeatShimmer = yebis.Config.HeatShimmer;
            LightShaft = yebis.Config.LightShaft;
            PreferredFormat = PixelFormat.R16G16B16A16_Float;

            // Make sure that the Depth Stencil will be created with ShaderResource
            Tags.Set(RenderTargetKeys.RequireDepthStencilShaderResource, true);
        }

        public ParameterCollection ViewParameters { get; set; }

        public PixelFormat PreferredFormat { get; set; }

        public Texture2D RenderSource { get; set; }

        public DepthStencilBuffer SourceDepth { get; set; }

        public RenderTarget RenderTarget { get; set; }

        private void CopyParametersToYebis()
        {
            // Retrieve defaults
            if (ViewParameters != null)
            {
                yebis.Config.Camera.NearClipPlane = ViewParameters.Get(CameraKeys.NearClipPlane);
                yebis.Config.Camera.FarClipPlane = ViewParameters.Get(CameraKeys.FarClipPlane);
                yebis.Config.Camera.FieldOfView = ViewParameters.Get(CameraKeys.FieldOfView);
                ViewParameters.Get(TransformationKeys.View, out yebis.Config.Camera.View);

                // Reverse value for Z and recalculate matrix projection
                yebis.Config.Camera.IsZReverse = yebis.Config.Camera.NearClipPlane > yebis.Config.Camera.FarClipPlane;
                float nearClipPlane = yebis.Config.Camera.IsZReverse ? yebis.Config.Camera.FarClipPlane : yebis.Config.Camera.NearClipPlane;
                float farClipPlane = yebis.Config.Camera.IsZReverse ? yebis.Config.Camera.NearClipPlane : yebis.Config.Camera.FarClipPlane;
                yebis.Config.Camera.NearClipPlane = nearClipPlane;
                yebis.Config.Camera.FarClipPlane = farClipPlane;

                var focusDistance = ViewParameters.Get(CameraKeys.FocusDistance);
                DepthOfField.AutoFocus = (focusDistance < 0.01);
                if (!DepthOfField.AutoFocus)
                {
                    DepthOfField.FocusDistance = focusDistance;
                }
            }

            yebis.Config.AntiAlias = AntiAlias;
            yebis.Config.ToneMap = ToneMap;
            yebis.Config.Glare = Glare;
            yebis.Config.ColorCorrection = ColorCorrection;
            yebis.Config.Lens = Lens;
            yebis.Config.DepthOfField = DepthOfField;
            yebis.Config.HeatShimmer = HeatShimmer;
            yebis.Config.LightShaft = LightShaft;
        }

        public override void Load()
        {
            base.Load();

            if (OfflineCompilation)
                return;

            RenderPass.StartPass += (context) =>
                {
                    CopyParametersToYebis();
                    yebis.Apply();

                    if (ToneMap.AutoExposure.Enable)
                        ToneMap.Exposure = yebis.Config.ToneMap.Exposure;

                    DepthOfField.FocusDistance = yebis.Config.DepthOfField.FocusDistance;
                };

            //var deviceContextPtr = RenderContext.GraphicsDeviceContext.NativeDeviceContext.NativePointer;

            //var renderTargetViewPtr = RenderTarget.NativeRenderTargetView.NativePointer;

            yebis.Initialize(GraphicsDevice, RenderTarget);
            yebis.SetSource(RenderSource, SourceDepth.Texture);
        }
    }
}
