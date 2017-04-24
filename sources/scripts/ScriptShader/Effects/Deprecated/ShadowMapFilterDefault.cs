// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class ShadowMapFilterDefault : ShadowMapFilter
    {
        public ShadowMapFilterDefault(ShadowMap shadowMap)
            : base(shadowMap)
        {
        }

        public override ShaderClassSource GenerateShaderSource(int shadowMapCount)
        {
            return new ShaderClassSource("ShadowMapFilterDefault");
        }
    }
}
