// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering.Skyboxes;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// A light coming from a skybox. The <see cref="SkyboxComponent"/> must be set on the entity in order to see a skybox. 
    /// </summary>
    [DataContract("LightSkybox")]
    [Display("Skybox")]
    public class LightSkybox : IEnvironmentLight
    {
        /// <summary>
        /// Gets the skybox component (this is set after the <see cref="LightProcessor"/> has processed this light.
        /// </summary>
        /// <value>The skybox component.</value>
        [DataMemberIgnore]
        public SkyboxComponent SkyboxComponent { get; private set; }

        [DataMemberIgnore]
        public Matrix SkyMatrix;

        public bool Update(LightComponent lightComponent)
        {
            SkyMatrix = Matrix.RotationQuaternion(lightComponent.Entity.Transform.Rotation);
            SkyboxComponent = lightComponent.Entity.Get<SkyboxComponent>();
            return SkyboxComponent != null && SkyboxComponent.Skybox != null;
        }
    }
}