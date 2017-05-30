// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class ShadowMapFilter
    {
        private ShadowMap shadowMap;

        protected ShadowMapFilter(ShadowMap shadowMap)
        {
            this.shadowMap = shadowMap;
        }

        public ShadowMap ShadowMap
        {
            get { return shadowMap; }
            internal set { shadowMap = value; }
        }

        public abstract ShaderClassSource GenerateShaderSource(int shadowMapCount);
    }
}
