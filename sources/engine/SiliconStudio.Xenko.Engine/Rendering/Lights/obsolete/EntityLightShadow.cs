// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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