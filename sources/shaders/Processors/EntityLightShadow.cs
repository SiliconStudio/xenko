// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Processors
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
                return Light.ShadowMap && ShadowMap != null && ShadowMap.Texture != null;
            }
        }
    }
}