// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Renderers;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A camera renderer.
    /// </summary>
    [DataContract("SceneUIRenderer")]
    [Display("Render UI")]
    public sealed class SceneUIRenderer : SceneEntityRenderer, IRendererManager
    {
        private readonly CameraRendererModeUI uiCameraRenderer = new CameraRendererModeUI();

        private readonly CameraComponentState cameraState = new CameraComponentState(new CameraComponent());

        private Vector2 lastTargetSize;

        private Int3 virtualResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraRenderer"/> class.
        /// </summary>
        public SceneUIRenderer()
        {
            ClearDepthBuffer = true;
            VirtualResolution = new Int3(1920, 1080, 1000);
        }

        /// <summary>
        /// Gets or sets the virtual resolution of the UI in virtual pixels.
        /// </summary>
        /// <userdoc>The value in pixels of the resolution of the UI</userdoc>
        [DataMember(10)]
        [Display("Virtual Resolution")]
        public Int3 VirtualResolution
        {
            get { return virtualResolution; }
            set
            {
                virtualResolution = value;
                lastTargetSize = new Vector2(-1);
            }
        }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        /// <userdoc>Indicate how the virtual resolution value should be interpreted</userdoc>
        [DataMember(20)]
        [Display("Virtual Resolution Mode")]
        public VirtualResolutionMode VirtualResolutionMode { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the depth buffer should be cleared before drawing.
        /// </summary>
        /// <userdoc>Indicate if the render should clear the current depth buffer before rendering.</userdoc>
        [DataMember(100)]
        [DefaultValue(true)]
        public bool ClearDepthBuffer { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            uiCameraRenderer.Initialize(Context);
        }

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            // Update UI camera and resolution if either VR or RenderTarget changed
            var targetSize = new Vector2(output.RenderTarget.Width, output.RenderTarget.Height);
            if (targetSize != lastTargetSize)
            {
                // update the virtual resolution of the renderer
                var virtualResolutionFloat = (Vector3)virtualResolution;
                if (VirtualResolutionMode == VirtualResolutionMode.WidthDepthTargetRatio)
                    virtualResolutionFloat.Y = virtualResolutionFloat.X * output.RenderTarget.Height / output.RenderTarget.Width;
                if (VirtualResolutionMode == VirtualResolutionMode.HeightDepthTargetRatio)
                    virtualResolutionFloat.X = virtualResolutionFloat.Y * output.RenderTarget.Width / output.RenderTarget.Height;

                uiCameraRenderer.VirtualResolution = virtualResolutionFloat;

                // Update the camera component state
                var nearPlane = 1f;
                var farPlane = nearPlane + 2 * virtualResolutionFloat.Z;
                var zOffset = virtualResolutionFloat.Z + 1f;
                var verticalFov = (float)Math.Atan2(virtualResolutionFloat.Y / 2, zOffset) * 2;
                cameraState.CameraComponent.AspectRatio = virtualResolutionFloat.X / virtualResolutionFloat.Y;
                cameraState.CameraComponent.VerticalFieldOfView = MathUtil.RadiansToDegrees(verticalFov);
                cameraState.View = Matrix.LookAtRH(new Vector3(0, 0, zOffset), Vector3.Zero, Vector3.UnitY);
                cameraState.Projection = Matrix.PerspectiveFovRH(verticalFov, cameraState.CameraComponent.AspectRatio, nearPlane, farPlane);

                lastTargetSize = targetSize;
            }

            // Draw this camera.
            using (context.PushTagAndRestore(Current, this))
            using (context.PushTagAndRestore(RenderFrame.Current, output))
            using (context.PushTagAndRestore(CameraComponentRenderer.Current, cameraState))
            {
                uiCameraRenderer.Draw(Context);
            }
        }

        protected override void Destroy()
        {
            base.Destroy();

            uiCameraRenderer.Dispose();
        }

        public ElementRenderer GetRenderer(UIElement element)
        {
            return uiCameraRenderer.GetRenderer(element);
        }

        public void RegisterRendererFactory(Type uiElementType, IElementRendererFactory factory)
        {
            uiCameraRenderer.RegisterRendererFactory(uiElementType, factory);
        }

        public void RegisterRenderer(UIElement element, ElementRenderer renderer)
        {
            uiCameraRenderer.RegisterRenderer(element, renderer);
        }
    }
}