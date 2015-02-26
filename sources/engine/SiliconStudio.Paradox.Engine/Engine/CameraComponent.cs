// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Describes the camera projection and view.
    /// </summary>
    [DataContract("CameraComponent")]
    [Display(130, "Camera")]
    [GizmoEntityFactory(GizmoEntityFactoryNames.CameraGizmoEntityFactoryQualifiedName)]
    [DefaultEntityComponentRenderer(typeof(CameraComponentRenderer), -1000)]
    public sealed class CameraComponent : EntityComponent
    {
        private float focusDistance;

        /// <summary>
        /// The property key of this component.
        /// </summary>
        public static PropertyKey<CameraComponent> Key = new PropertyKey<CameraComponent>("Key", typeof(CameraComponent));

        /// <summary>
        /// Create a new <see cref="CameraComponent"/> instance.
        /// </summary>
        public CameraComponent()
            : this(null, 0.1f , 1000.0f)
        {
        }

        /// <summary>
        /// Create a new <see cref="CameraComponent"/> instance with the provided target, near plane and far plane. 
        /// </summary>
        /// <param name="target">The entity to use as target.</param>
        /// <param name="nearPlane">The near plane value</param>
        /// <param name="farPlane">The far plane value</param>
        public CameraComponent(Entity target, float nearPlane, float farPlane)
        {
            Projection = CameraProjectionMode.Perspective;
            VerticalFieldOfView = 45.0f;
            OrthographicSize = 10.0f;

            // TODO: Handle Aspect ratio differently
            AspectRatio = 16f / 9f;
            Target = target;
            TargetUp = Vector3.UnitY;
            NearPlane = nearPlane;
            FarPlane = farPlane;
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
        [DefaultValue(45.0f)]
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
        [DefaultValue(10.0f)]
        [Display("Orthographic Size")]
        public float OrthographicSize { get; set; }

        /// <summary>
        /// Gets or sets the near plane distance.
        /// </summary>
        /// <value>
        /// The near plane distance.
        /// </value>
        [DataMember(20)]
        [DefaultValue(0.1f)]
        public float NearPlane { get; set; }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
        /// <value>
        /// The far plane distance.
        /// </value>
        [DataMember(30)]
        [DefaultValue(1000.0f)]
        public float FarPlane { get; set; }

        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>
        /// The aspect ratio.
        /// </value>
        [DataMemberIgnore]
        public float AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the target this camera is pointing to. May be null.
        /// </summary>
        /// <value>The target.</value>
        [DataMemberIgnore]
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets the up direction when using a target (for LookAt).
        /// </summary>
        /// <value>
        /// The up direction when using a target (for LookAt).
        /// </value>
        [DataMemberIgnore]
        public Vector3 TargetUp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto focus].
        /// </summary>
        /// <value><c>true</c> if [auto focus]; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool AutoFocus { get; set; }

        /// <summary>
        /// Gets or sets the focus distance.
        /// </summary>
        /// <value>The focus distance.</value>
        [DataMemberIgnore]
        public float FocusDistance
        {
            get
            {
                if (AutoFocus)
                    return 0.0f;

                if (Entity != null && Target != null)
                {
                    var eye = Entity.Transform.WorldMatrix.TranslationVector;
                    var target = Target.Transform.WorldMatrix.TranslationVector;
                    return Vector3.Distance(eye, target);
                }

                return focusDistance;
            }

            set
            {
                if (AutoFocus)
                {
                    return;
                }

                if (Entity == null || Target == null)
                {
                    focusDistance = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom <see cref="ViewMatrix"/>. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if use custom <see cref="ViewMatrix"/>; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool UseViewMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local view matrix, only used when <see cref="UseViewMatrix"/> is <c>true</c>.
        /// </summary>
        /// <value>The local view matrix.</value>
        [DataMemberIgnore]
        public Matrix ViewMatrix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom <see cref="ProjectionMatrix"/>. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if use custom <see cref="ProjectionMatrix"/>; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool UseProjectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local projection matrix, only used when <see cref="UseProjectionMatrix"/> is <c>true</c>.
        /// </summary>
        /// <value>The local projection matrix.</value>
        [DataMemberIgnore]
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Calculates the projection matrix and view matrix.
        /// </summary>
        /// <param name="projection">The projection matrix.</param>
        /// <param name="viewMatrix">The view matrix.</param>
        public void Calculate(out Matrix projection, out Matrix viewMatrix)
        {
            // Calculates the View
            if (UseViewMatrix)
            {
                // We are using a ViewMatrix Matrix that is overriding Entity/Target matrices
                viewMatrix = ViewMatrix;
            }
            else
            {
                if (Target != null)
                {
                    // Build a view matrix from the Entity position and Target
                    // Currently use Y in camera local space as Up axis (need separate TargetUp that we multiply with WorldMatrix?)
                    var transformation = EnsureEntity.Transform;
                    var targetUp = TargetUp;
                    Vector3.TransformNormal(ref targetUp, ref transformation.WorldMatrix, out targetUp);
                    viewMatrix = Matrix.LookAtRH(transformation.WorldMatrix.TranslationVector, Target.Transform.WorldMatrix.TranslationVector, targetUp);
                }
                else
                {
                    // TODO: determine which axis of the camera to look from
                    var worldMatrix = EnsureEntity.Transform.WorldMatrix;
                    Matrix.Invert(ref worldMatrix, out viewMatrix);
                }
            }
            
            // Calculates the projection
            // TODO: Should we throw an error if Projection is not set?
            if (UseProjectionMatrix)
            {
                projection = ProjectionMatrix;
            }
            else
            {
                projection = Projection == CameraProjectionMode.Perspective ? Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(VerticalFieldOfView), AspectRatio, NearPlane, FarPlane) : Matrix.OrthoRH(OrthographicSize, OrthographicSize, NearPlane, FarPlane);
            }
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}