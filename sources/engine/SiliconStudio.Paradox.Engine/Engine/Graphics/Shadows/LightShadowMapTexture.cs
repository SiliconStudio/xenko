// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    /// <summary>
    /// An allocated shadow map texture associated to a light.
    /// </summary>
    public struct LightShadowMapTexture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMapTexture"/> struct.
        /// </summary>
        /// <param name="light">The light component.</param>
        /// <param name="shadowMap">The light.</param>
        /// <param name="size">The shadow map.</param>
        /// <param name="size">The size.</param>
        public LightShadowMapTexture(LightComponent lightComponent, IDirectLight light, LightShadowMap shadowMap, int size)
            : this()
        {
            if (lightComponent == null) throw new ArgumentNullException("lightComponent");
            if (light == null) throw new ArgumentNullException("light");
            if (shadowMap == null) throw new ArgumentNullException("shadowMap");
            LightComponent = lightComponent;
            Light = light;
            Shadow = shadowMap;
            Size = size;
            FilterType = Shadow.Filter == null || !Shadow.Filter.RequiresCustomBuffer() ? null : Shadow.Filter.GetType();
        }

        public readonly LightComponent LightComponent;

        public readonly IDirectLight Light;

        public readonly LightShadowMap Shadow;

        public readonly Type FilterType;

        public readonly int Size;

        public int CascadeCount { get; set; }

        public ShadowMapAtlasTexture Atlas { get; internal set; }

        public ILightShadowMapRenderer Renderer;

        public Rectangle GetRectangle(int i)
        {
            if (i < 0 || i > CascadeCount)
            {
                throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
            }
            unsafe
            {
                fixed (void* ptr = &Rectangle0)
                {
                    return ((Rectangle*)ptr)[i];
                }
            }
        }

        public void SetRectangle(int i, Rectangle value)
        {
            if (i < 0 || i > CascadeCount)
            {
                throw new ArgumentOutOfRangeException("i", "Must be in the range [0, CascadeCount[");
            }
            unsafe
            {
                fixed (void* ptr = &Rectangle0)
                {
                    ((Rectangle*)ptr)[i] = value;
                }
            }
        }

        private Rectangle Rectangle0;

        private Rectangle Rectangle1;

        private Rectangle Rectangle2;

        private Rectangle Rectangle3;
    }
}