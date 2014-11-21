// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Describes the camera projection and view.
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    [DataContract("CameraComponent")]
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
            : this(null, 0 ,0)
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
            AspectRatio = 16f / 9f;
            Target = target;
            TargetUp = Vector3.UnitY;
            NearPlane = nearPlane;
            FarPlane = farPlane;
            VerticalFieldOfView = (float)Math.PI * 0.4f;
        }

        /// <summary>
        /// Associates an entity with this camera component.
        /// </summary>
        /// <param name="name">The name of entity.</param>
        /// <returns>This CameraComponent.</returns>
        [Obsolete("This method will be removed in a future release")]
        public CameraComponent WithEntity(string name)
        {
            // By default create an entity on the CameraComponent
            // This can be overrident later
            var entity = new Entity(name);
            entity.Add(this);

            Entity = entity;
            return this;
        }

        /// <summary>
        /// Gets or sets the vertical field of view.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        [DataMemberConvert]
        public float VerticalFieldOfView { get; set; }

        /// <summary>
        /// Gets or sets the near plane distance.
        /// </summary>
        /// <value>
        /// The near plane distance.
        /// </value>
        [DataMemberConvert]
        public float NearPlane { get; set; }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
        /// <value>
        /// The far plane distance.
        /// </value>
        [DataMemberConvert]
        public float FarPlane { get; set; }

        /// <summary>
        /// Gets or sets the aspect ratio.
        /// </summary>
        /// <value>
        /// The aspect ratio.
        /// </value>
        [DataMemberConvert]
        public float AspectRatio { get; set; }

        /// <summary>
        /// Gets or sets the target this camera is pointing to. May be null.
        /// </summary>
        /// <value>The target.</value>
        [DataMemberConvert]
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets the up direction when using a target (for LookAt).
        /// </summary>
        /// <value>
        /// The up direction when using a target (for LookAt).
        /// </value>
        [DataMemberConvert]
        public Vector3 TargetUp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto focus].
        /// </summary>
        /// <value><c>true</c> if [auto focus]; otherwise, <c>false</c>.</value>
        [DataMemberConvert]
        public bool AutoFocus { get; set; }

        /// <summary>
        /// Gets or sets the focus distance.
        /// </summary>
        /// <value>The focus distance.</value>
        [DataMemberConvert]
        public float FocusDistance
        {
            get
            {
                if (AutoFocus)
                    return 0.0f;

                if (Entity != null && Target != null)
                {
                    var eye = Entity.Transformation.WorldMatrix.TranslationVector;
                    var target = Target.Transformation.WorldMatrix.TranslationVector;
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
        [DataMemberConvert]
        public bool UseViewMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local view matrix, only used when <see cref="UseViewMatrix"/> is <c>true</c>.
        /// </summary>
        /// <value>The local view matrix.</value>
        [DataMemberConvert]
        public Matrix ViewMatrix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use custom <see cref="ProjectionMatrix"/>. Default is <c>false</c>
        /// </summary>
        /// <value><c>true</c> if use custom <see cref="ProjectionMatrix"/>; otherwise, <c>false</c>.</value>
        [DataMemberConvert]
        public bool UseProjectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets the local projection matrix, only used when <see cref="UseProjectionMatrix"/> is <c>true</c>.
        /// </summary>
        /// <value>The local projection matrix.</value>
        [DataMemberConvert]
        public Matrix ProjectionMatrix { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position.</value>
        public Vector3 Position
        {
            get
            {
                return EnsureEntity.Transformation.Translation;
            }

            set
            {
                EnsureEntity.Transformation.Translation = value;
            }
        }

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
                    var transformation = EnsureEntity.Transformation;
                    var targetUp = TargetUp;
                    Vector3.TransformNormal(ref targetUp, ref transformation.WorldMatrix, out targetUp);
                    viewMatrix = Matrix.LookAtRH(transformation.WorldMatrix.TranslationVector, Target.Transformation.WorldMatrix.TranslationVector, targetUp);
                }
                else
                {
                    // TODO: determine which axis of the camera to look from
                    var worldMatrix = EnsureEntity.Transformation.WorldMatrix;
                    Matrix.Invert(ref worldMatrix, out viewMatrix);
                }
            }
            
            // Calculates the projection
            if (UseProjectionMatrix)
            {
                projection = ProjectionMatrix;
            }
            else
            {
                Matrix.PerspectiveFovRH(VerticalFieldOfView, AspectRatio, NearPlane, FarPlane, out projection);
            }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }

    public static class CameraComponentExtensions
    {
        public static CameraComponent GetCamera(this RenderPass pass)
        {
            return pass.GetProcessor<CameraSetter>().Camera;
        }

        public static void SetCamera(this RenderPass pass, CameraComponent camera)
        {
            pass.GetProcessor<CameraSetter>().Camera = camera;
        }
    }
}