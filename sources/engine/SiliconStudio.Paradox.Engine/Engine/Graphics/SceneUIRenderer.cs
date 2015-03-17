// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A camera renderer.
    /// </summary>
    [DataContract("SceneUIRenderer")]
    [Display("Render UI")]
    public sealed class SceneUIRenderer : SceneEntityRenderer
    {
        private readonly CameraRendererModeForward cameraRenderer = new CameraRendererModeForward();

        private readonly CameraComponent cameraComponent = new CameraComponent();

        private Vector2 lastTargetSize;

        private Int3 virtualResolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraRenderer"/> class.
        /// </summary>
        public SceneUIRenderer()
        {
            ClearDepthBuffer = true;
            VirtualResolution = new Int3(1920, 1080, 1000);
            VirtualResolutionMode = VirtualResolutionMode.HeightDepthTargetRatio;
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
        [DefaultValue(VirtualResolutionMode.HeightDepthTargetRatio)]
        public VirtualResolutionMode VirtualResolutionMode { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the depth buffer should be cleared before drawing.
        /// </summary>
        /// <userdoc>Indicate if the render should clear the current depth buffer before rendering.</userdoc>
        [DataMember(100)]
        [DefaultValue(true)]
        public bool ClearDepthBuffer { get; set; }

        /// <summary>
        /// Gets the UI component renderer used by the scene renderer.
        /// </summary>
        [DataMemberIgnore]
        public UIComponentRenderer UIRenderer { get { return cameraRenderer.Renderers.Count>0? (UIComponentRenderer)cameraRenderer.Renderers[0]: null; } }

        [DataMemberIgnore]
        internal Vector3 VirtualResolutionFactor { get; set; }
        
        protected override void InitializeCore()
        {
            base.InitializeCore();

            cameraRenderer.RenderComponentTypes.Add(typeof(UIComponent));
            cameraRenderer.Initialize(Context);
        }

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            // Update UI camera and resolution if either VR or RenderTarget changed
            var renderTarget = output.RenderTargets[0]; // TODO avoid hardcoded target
            var targetSize = new Vector2(renderTarget.Width, renderTarget.Height);
            if (targetSize != lastTargetSize)
            {
                // update the virtual resolution of the renderer
                var virtualResolutionFloat = (Vector3)virtualResolution;
                if (VirtualResolutionMode == VirtualResolutionMode.WidthDepthTargetRatio)
                    virtualResolutionFloat.Y = virtualResolutionFloat.X * targetSize.Y / targetSize.X;
                if (VirtualResolutionMode == VirtualResolutionMode.HeightDepthTargetRatio)
                    virtualResolutionFloat.X = virtualResolutionFloat.Y * targetSize.X / targetSize.Y;

                VirtualResolutionFactor = virtualResolutionFloat;

                // Update the camera component state
                var nearPlane = 1f;
                var farPlane = nearPlane + 2 * virtualResolutionFloat.Z;
                var zOffset = virtualResolutionFloat.Z + 1f;
                var verticalFov = (float)Math.Atan2(virtualResolutionFloat.Y / 2, zOffset) * 2;
                cameraComponent.AspectRatio = virtualResolutionFloat.X / virtualResolutionFloat.Y;
                cameraComponent.VerticalFieldOfView = MathUtil.RadiansToDegrees(verticalFov);
                cameraComponent.ViewMatrix = Matrix.LookAtRH(new Vector3(0, 0, zOffset), Vector3.Zero, Vector3.UnitY);
                cameraComponent.ProjectionMatrix = Matrix.PerspectiveFovRH(verticalFov, cameraComponent.AspectRatio, nearPlane, farPlane);
                Matrix.Multiply(ref cameraComponent.ViewMatrix, ref cameraComponent.ProjectionMatrix, out cameraComponent.ViewProjectionMatrix);

                lastTargetSize = targetSize;
            }

            // Draw this camera.
            using (context.PushTagAndRestore(Current, this))
            using (context.PushTagAndRestore(RenderFrame.Current, output))
            using (context.PushTagAndRestore(CameraComponentRenderer.Current, cameraComponent))
            {
                cameraRenderer.Draw(Context);
            }
        }

        protected override void Destroy()
        {
            base.Destroy();

            cameraRenderer.Dispose();
        }
    }
}