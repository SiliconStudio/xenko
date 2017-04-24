// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Effects.Lights;
using SiliconStudio.Xenko.Effects.Shadows;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;

namespace SiliconStudio.Xenko.Effects.Processors
{
    public class EntityLightShadow
    {
        public Entity Entity;

        public LightComponent Light;

        public ShadowMap ShadowMap;

        public bool HasShadowMap
        {
            get
            {
                return Light.Shadow != null && ShadowMap != null && ShadowMap.Texture != null;
            }
        }
    }
}
