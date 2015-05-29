// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IDirectLight"/>.
    /// </summary>
    [DataContract()]
    public abstract class DirectLightBase : ColorLightBase, IDirectLight
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectLightBase"/> class.
        /// </summary>
        protected DirectLightBase()
        {
        }

        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        [Category]
        [DataMember(200)]
        public LightShadowMap Shadow { get; set; }

        public abstract bool HasBoundingBox { get; }

        public abstract BoundingBox ComputeBounds(Vector3 position, Vector3 direction);

        public float ComputeScreenCoverage(RenderContext context, Vector3 position, Vector3 direction)
        {
            var viewport = context.GraphicsDevice.Viewport;
            var camera = context.GetCurrentCamera();
            if (camera == null)
            {
                return 0.0f;
            }
            return ComputeScreenCoverage(camera, position, direction, viewport.Width, viewport.Height);
        }

        protected abstract float ComputeScreenCoverage(CameraComponent camera, Vector3 position, Vector3 direction, float width, float height);
    }
}