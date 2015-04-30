// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Gizmos;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Describes the camera projection and view.
    /// </summary>
    [DataContract("CameraComponent")]
    [Display(130, "Camera")]
    [GizmoEntity(GizmoEntityNames.CameraGizmoEntityQualifiedName)]
    [DefaultEntityComponentRenderer(typeof(CameraComponentRenderer), -1000)]
    public sealed class CameraComponent : EntityComponent
    {
        public const float DefaultAspectRatio = 16.0f / 9.0f;

        public const float DefaultOrthographicSize = 10.0f;

        public const float DefaultVerticalFieldOfView = 45.0f;

        public const float DefaultNearClipPlane = 0.1f;

        public const float DefaultFarClipPlane = 1000.0f;


        /// <summary>
        /// The property key of this component.
        /// </summary>
        public static PropertyKey<CameraComponent> Key = new PropertyKey<CameraComponent>("Key", typeof(CameraComponent));

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

            // TODO: Handle Aspect ratio differently
            AspectRatio = DefaultAspectRatio;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>The projection.</value>
        [DataMember(0)]
        [NotNull]
        public CameraProjectionMode Projection { get; set; }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        [DataMember(5)]
        [DefaultValue(DefaultVerticalFieldOfView)]
        [Display("Field Of View")]
        [DataMemberRange(1.0, 179.0, 1.0, 10.0, 0)]
        public float VerticalFieldOfView { get; set; }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
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
        [DataMember(20)]
        [DefaultValue(DefaultNearClipPlane)]
        public float NearClipPlane { get; set; }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
        /// <value>
        /// The far plane distance.
        /// </value>
        [DataMember(30)]
        [DefaultValue(DefaultFarClipPlane)]
        public float FarClipPlane { get; set; }

        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>
        /// The aspect ratio.
        /// </value>
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
            // Calculates the View
            if (!UseCustomViewMatrix)
            {
                var worldMatrix = EnsureEntity.Transform.WorldMatrix;
                Matrix.Invert(ref worldMatrix, out ViewMatrix);
            }
            
            // Calculates the projection
            // TODO: Should we throw an error if Projection is not set?
            if (!UseCustomProjectionMatrix)
            {
                ProjectionMatrix = Projection == CameraProjectionMode.Perspective ? 
                    Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(VerticalFieldOfView), AspectRatio, NearClipPlane, FarClipPlane) :
                    Matrix.OrthoRH(AspectRatio * OrthographicSize, OrthographicSize, NearClipPlane, FarClipPlane);
            }

            // Update ViewProjectionMatrix
            Matrix.Multiply(ref ViewMatrix, ref ProjectionMatrix, out ViewProjectionMatrix);

            // Update the frustum.
            Frustum = new BoundingFrustum(ref ViewProjectionMatrix);
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}