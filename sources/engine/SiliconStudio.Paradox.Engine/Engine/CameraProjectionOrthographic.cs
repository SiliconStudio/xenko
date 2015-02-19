// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// An orthographic camera projection.
    /// </summary>
    [DataContract("CameraProjectionOrthographic")]
    [Display("Orthographic")]
    public sealed class CameraProjectionOrthographic : ICameraProjection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProjectionOrthographic"/> class.
        /// </summary>
        public CameraProjectionOrthographic() : this(10.0f)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraProjectionOrthographic" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public CameraProjectionOrthographic(float size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets or sets the vertical field of view in degrees.
        /// </summary>
        /// <value>
        /// The vertical field of view.
        /// </value>
        [DataMember(10)]
        [DefaultValue(10.0f)]
        public float Size { get; set; }

        public Matrix CalculateProjection(float aspectRatio, float nearPlane, float farPlane)
        {
            return Matrix.OrthoRH(Size, Size, nearPlane, farPlane);
        }
    }
}