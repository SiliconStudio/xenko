// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Skyboxes;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// A light coming from a skybox. The <see cref="SkyboxComponent"/> must be set on the entity in order to see a skybox. 
    /// </summary>
    [DataContract("LightSkybox")]
    [Display("Skybox")]
    public class LightSkybox : IEnvironmentLight
    {
        /// <summary>
        /// Gets the skybox (this is set after the <see cref="LightProcessor"/> has processed this light.
        /// </summary>
        /// <value>The skybox.</value>
        [DataMember(0)]
        public Skybox Skybox { get; set; }

        [DataMemberIgnore]
        internal Quaternion Rotation;

        public bool Update(LightComponent lightComponent)
        {
            Rotation = Quaternion.RotationMatrix(lightComponent.Entity.Transform.WorldMatrix);
            return Skybox != null;
        }
    }
}
