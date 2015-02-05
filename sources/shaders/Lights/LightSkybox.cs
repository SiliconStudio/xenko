// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects.Skyboxes;

namespace SiliconStudio.Paradox.Effects.Lights
{
    /// <summary>
    /// A light coming from a skybox. The <see cref="SkyboxComponent"/> must be set on the entity in order to see a skybox. 
    /// </summary>
    [DataContract("LightSkybox")]
    [Display("Skybox")]
    public class LightSkybox : IEnvironmentLight
    {
    }
}