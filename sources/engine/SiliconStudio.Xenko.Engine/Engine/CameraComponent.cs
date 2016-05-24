// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Describes the camera projection and view.
    /// </summary>
    [DataContract("CameraComponent")]
    [Display("Camera", Expand = ExpandRule.Once)]
    //[DefaultEntityComponentRenderer(typeof(CameraComponentRenderer), -1000)]
    [ComponentOrder(13000)]
    public sealed class CameraComponent : ActivableEntityComponent
    {
        public const float DefaultAspectRatio = 16.0f / 9.0f;

        public const float DefaultOrthographicSize = 10.0f;

        public const float DefaultVerticalFieldOfView = 45.0f;

        public const float DefaultNearClipPlane = 0.1f;

        public const float DefaultFarClipPlane = 1000.0f;

        /// <summary>
        /// Create a new <see cref="CameraComponent"/> instance.
        /// </summary>
        public CameraComponent()
            : this(DefaultNearClipPlane, DefaultFarClipPlane)
        {
        }

        /// <summary>
        /// Create a new <see cref="CameraComponent" /> instance with the provided target, near plane and far plane.
        /// </summary>
        /// <param name="nearClipPlane">The near plane value</param>
        /// <param name="farClipPlane">The far plane value</param>
        public CameraComponent(float nearClipPlane, float farClipPlane)
        {
            Projection = CameraProjectionMode.Perspective;
            VerticalFieldOfView = DefaultVerticalFieldOfView;
            OrthographicSize = DefaultOrthographicSize;
            AspectRatio = DefaultAspectRatio;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>The projection.</value>
        /// <userdoc>The type of projection used by the camera.</userdoc>
        [DataMember(0)]
        [NotNull]
        public CameraProjectionMode Projection { get; set; }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        /// <userdoc>The vertical field-of-view used by the camera (in degrees).</userdoc>
        [DataMember(5)]
        [DefaultValue(DefaultVerticalFieldOfView)]
        [Display("Field Of View")]
        [DataMemberRange(1.0, 179.0, 1.0, 10.0, 0)]
        public float VerticalFieldOfView { get; set; }

        /// <summary>
        /// Gets or sets the height of the orthographic projection.
        /// </summary>
        /// <value>
        /// The height of the orthographic projection.
        /// </value>
        /// <userdoc>The height of the orthographic projection (the width is automatically calculated based on the target ratio).</userdoc>
        [DataMember(10)]
        [DefaultValue(DefaultOrthographicSize)]
        [Display("Orthographic Size")]
        public float OrthographicSize { get; set; }

        /// <summary>
        /// Gets or sets the near plane distance.
        /// </summary>
        /// <value>
        /// The near plane distance.
        /// </value>
        /// <userdoc>The value of the near clip plane.</userdoc>
        [DataMember(20)]
        [DefaultValue(DefaultNearClipPlane)]
        public float NearClipPlane { get; set; }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
        /// <value>
        /// The far plane distance.
        /// </value>
        /// <userdoc>The value of the far clip plane.</userdoc>
        [DataMember(30)]
        [DefaultValue(DefaultFarClipPlane)]
        public float FarClipPlane { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use a custom <see cref="AspectRatio"/>. Default is <c>false</c>, meaning that the aspect ratio is calculated from the ratio of the current viewport when rendering.
        /// </summary>
        /// <value>The use custom aspect ratio.</value>
        /// <userdoc>If checked, use the value contained in 'Aspect Ratio' to calculate the projection matrices. Otherwise, automatically adjust the aspect ratio to the ratio of the render target.</userdoc>
        [DataMember(35)]
        [DefaultValue(false)]
        [Display("Custom Aspect Ratio")]
        public bool UseCustomAspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>
        /// The aspect ratio.
        /// </value>
        /// <userdoc>The aspect ratio used to build the projection matrices when 'Custom Aspect Ratio?' is checked.</userdoc>
        [DataMember(40)]
        [DefaultValue(DefaultAspectRatio)]
        public float AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom <see cref="ViewMatrix"/>. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if use custom <see cref="ViewMatrix"/>; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool UseCustomViewMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local view matrix. See remarks.
        /// </summary>
        /// <value>The local view matrix.</value>
        /// <remarks>
        /// This value is updated when calling <see cref="Update"/> or is directly used when <see cref="UseCustomViewMatrix"/> is <c>true</c>.
        /// </remarks>
        [DataMemberIgnore]
        public Matrix ViewMatrix;

        /// <summary>
        /// Gets or sets a value indicating whether to use custom <see cref="ProjectionMatrix"/>. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if use custom <see cref="ProjectionMatrix"/>; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool UseCustomProjectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local projection matrix. See remarks.
        /// </summary>
        /// <value>The local projection matrix.</value>
        /// <remarks>
        /// This value is updated when calling <see cref="Update"/> or is directly used when <see cref="UseCustomViewMatrix"/> is <c>true</c>.
        /// </remarks>
        [DataMemberIgnore]
        public Matrix ProjectionMatrix;

        /// <summary>
        /// The view projection matrix calculated automatically after calling <see cref="Update"/> method.
        /// </summary>
        [DataMemberIgnore]
        public Matrix ViewProjectionMatrix;

        /// <summary>
        /// The frustum extracted from the view projection matrix calculated automatically after calling <see cref="Update"/> method.
        /// </summary>
        [DataMemberIgnore]
        public BoundingFrustum Frustum;

        /// <summary>
        /// Calculates the projection matrix and view matrix.
        /// </summary>
        public void Update()
        {
            Update(null);
        }

        /// <summary>
        /// Calculates the projection matrix and view matrix.
        /// </summary>
        /// <param name="screenAspectRatio">The current screen aspect ratio. If null, use the <see cref="AspectRatio"/> even if <see cref="UseCustomAspectRatio"/> is false.</param>
        public void Update(float? screenAspectRatio)
        {
            // Calculates the View
            if (!UseCustomViewMatrix)
            {
                var worldMatrix = EnsureEntity.Transform.WorldMatrix;

                Vector3 scale, translation;
                worldMatrix.Decompose(out scale, out ViewMatrix, out translation);

                // Transpose ViewMatrix (rotation only, so equivalent to inversing it)
                ViewMatrix.Transpose();

                // Rotate our translation so that we can inject it in the view matrix directly
                Vector3.TransformCoordinate(ref translation, ref ViewMatrix, out translation);

                // Apply inverse of translation (equivalent to opposite)
                ViewMatrix.TranslationVector = -translation;
            }
            
            // Calculates the projection
            // TODO: Should we throw an error if Projection is not set?
            if (!UseCustomProjectionMatrix)
            {
                // Calculates the aspect ratio
                var aspectRatio = AspectRatio;
                if (screenAspectRatio.HasValue && !UseCustomAspectRatio)
                {
                    aspectRatio = screenAspectRatio.Value;
                }

                ProjectionMatrix = Projection == CameraProjectionMode.Perspective ?
                    Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(VerticalFieldOfView), aspectRatio, NearClipPlane, FarClipPlane) :
                    Matrix.OrthoRH(aspectRatio * OrthographicSize, OrthographicSize, NearClipPlane, FarClipPlane);
            }

            // Update ViewProjectionMatrix
            Matrix.Multiply(ref ViewMatrix, ref ProjectionMatrix, out ViewProjectionMatrix);

            // Update the frustum.
            Frustum = new BoundingFrustum(ref ViewProjectionMatrix);
        }
    }
}