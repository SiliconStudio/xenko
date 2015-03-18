// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Settings for the Camera.
    /// </summary>
    [DataContract("SceneEditorCameraSettings")]
    public sealed class SceneEditorCameraSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEditorSettings" /> class.
        /// </summary>
        public SceneEditorCameraSettings()
        {
            Projection = CameraProjectionMode.Perspective;
            VerticalFieldOfView = 45.0f;
            OrthographicSize = 10.0f;
            NearPlane = 10f;
            FarPlane = 200000f;
        }

        /// <summary>
        /// Gets or sets the projection.
        /// </summary>
        /// <value>The projection.</value>
        [DataMember(20)]
        [DefaultValue(CameraProjectionMode.Perspective)]
        public CameraProjectionMode Projection { get; set; }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        [DataMember(30)]
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
        [DataMember(40)]
        [DefaultValue(10.0f)]
        [Display("Orthographic Size")]
        public float OrthographicSize { get; set; }

        /// <summary>
        /// Gets or sets the near plane distance.
        /// </summary>
        /// <value>
        /// The near plane distance.
        /// </value>
        [DataMember(50)]
        [DefaultValue(10f)]
        public float NearPlane { get; set; }

        /// <summary>
        /// Gets or sets the far plane distance.
        /// </summary>
        /// <value>
        /// The far plane distance.
        /// </value>
        [DataMember(60)]
        [DefaultValue(200000f)]
        public float FarPlane { get; set; }

        /// <summary>
        /// Copies main parameters from this component to the specified instance.
        /// </summary>
        /// <param name="camera">The camera to receive copied parameters from this instance.</param>
        /// <exception cref="System.ArgumentNullException">camera</exception>
        public void CopyTo(CameraComponent camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            camera.Projection = Projection;
            camera.VerticalFieldOfView = VerticalFieldOfView;
            camera.OrthographicSize = OrthographicSize;
            camera.NearPlane = NearPlane;
            camera.FarPlane = FarPlane;
            // TODO: Aspect ratio
        }
    }
}