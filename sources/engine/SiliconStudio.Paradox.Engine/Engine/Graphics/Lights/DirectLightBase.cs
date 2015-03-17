// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IDirectLight"/>.
    /// </summary>
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
        [DataMember(200)]
        [DefaultValue(null)]
        public ILightShadow Shadow { get; set; }

        /// <summary>
        /// Gets the importance of the shadow. See remarks.
        /// </summary>
        /// <value>The shadow importance.</value>
        /// <returns>System.Single.</returns>
        /// <remarks>The higher the importance is, the higher the cost of shadow computation is costly</remarks>
        [DataMember(210)]
        public LightShadowImportance ShadowImportance { get; set; }

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