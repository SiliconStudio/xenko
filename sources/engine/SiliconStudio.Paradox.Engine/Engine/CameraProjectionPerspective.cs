// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A perspective camera projection.
    /// </summary>
    [DataContract("CameraProjectionPerspective")]
    [Display("Perspective")]
    public sealed class CameraProjectionPerspective : ICameraProjection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProjectionPerspective"/> class.
        /// </summary>
        public CameraProjectionPerspective()
        {
            VerticalFieldOfView = 45.0f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProjectionPerspective"/> class.
        /// </summary>
        /// <param name="verticalFieldOfView">The vertical field of view.</param>
        public CameraProjectionPerspective(float verticalFieldOfView)
        {
            VerticalFieldOfView = verticalFieldOfView;
        }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        [DataMember(10)]
        [DefaultValue(45.0f)]
        [Display("Field Of View")]
        [DataMemberRange(1.0, 179.0, 1.0, 10.0, 0)]
        public float VerticalFieldOfView { get; set; }

        public Matrix CalculateProjection(float aspectRatio, float nearPlane, float farPlane)
        {
            return Matrix.PerspectiveFovRH(MathUtil.DegreesToRadians(VerticalFieldOfView), aspectRatio, nearPlane, farPlane);
        }
    }
}