// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base implementation of <see cref="IDirectLight"/>.
    /// </summary>
    [DataContract()]
    public abstract class DirectLightBase : ColorLightBase, IDirectLight
    {
        /// <summary>
        /// Gets or sets the shadow.
        /// </summary>
        /// <value>The shadow.</value>
        /// <userdoc>The settings of the light shadow</userdoc>
        [DataMember(200)]
        public LightShadowMap Shadow { get; protected set; }

        public abstract bool HasBoundingBox { get; }

        public abstract BoundingBox ComputeBounds(Vector3 position, Vector3 direction);

        public abstract float ComputeScreenCoverage(RenderView renderView, Vector3 position, Vector3 direction);
    }
}
