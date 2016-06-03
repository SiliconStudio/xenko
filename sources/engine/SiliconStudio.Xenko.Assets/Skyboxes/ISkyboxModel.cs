// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Assets.Skyboxes
{
    /// <summary>
    /// Interface for a skybox model (cubemap, cubecross, latlong, facelist...etc.)
    /// </summary>
    public interface ISkyboxModel
    {
        ShaderSource Generate(SkyboxGeneratorContext context);

        IEnumerable<IReference> GetDependencies();
    }
}
