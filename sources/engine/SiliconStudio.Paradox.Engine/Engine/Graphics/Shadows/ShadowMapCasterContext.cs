// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    /// <summary>
    /// Context used when rendering a shadow map.
    /// </summary>
    public class ShadowMapCasterContext
    {
        /// <summary>
        /// Base points for frustum corners.
        /// </summary>
        private static readonly Vector3[] FrustumBasePoints =
        {
            new Vector3(-1.0f,-1.0f,-1.0f), new Vector3(1.0f,-1.0f,-1.0f), new Vector3(-1.0f,1.0f,-1.0f), new Vector3(1.0f,1.0f,-1.0f),
            new Vector3(-1.0f,-1.0f, 1.0f), new Vector3(1.0f,-1.0f, 1.0f), new Vector3(-1.0f,1.0f, 1.0f), new Vector3(1.0f,1.0f, 1.0f),
        };

        public ShadowMapCasterContext(CameraComponent camera)
        {
            if (camera == null) throw new ArgumentNullException("camera");
            Camera = camera;

            FrustumCorner = new Vector3[8];
            FrustumDirection = new Vector3[4];

            Initialize(camera);
        }

        /// <summary>
        /// The frustum corner positions in world space
        /// </summary>
        public readonly Vector3[] FrustumCorner;

        /// <summary>
        /// The frustum direction in world space
        /// </summary>
        public readonly Vector3[] FrustumDirection;

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        public CameraComponent Camera { get; private set; }

        public void Initialize(CameraComponent camera)
        {
            Camera = camera;

            // Compute frustum-dependent variables (common for all shadow maps)
            Matrix projectionToWorld;
            Matrix.Invert(ref camera.ViewProjectionMatrix, out projectionToWorld);

            // Transform Frustum corners in World Space (8 points) - algorithm is valid only if the view matrix does not do any kind of scale/shear transformation
            for (int i = 0; i < 8; ++i)
            {
                Vector3.TransformCoordinate(ref FrustumBasePoints[i], ref projectionToWorld, out FrustumCorner[i]);
            }

            // Compute frustum edge directions
            for (int i = 0; i < 4; i++)
            {
                FrustumDirection[i] = Vector3.Normalize(FrustumCorner[i + 4] - FrustumCorner[i]);
            }
        }
    }
}