// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Lights;

namespace SiliconStudio.Paradox.Effects.Shadows
{
    /// <summary>
    /// An allocated shadow map texture associated to a light.
    /// </summary>
    public class LightShadowMapTexture
    {
        private static readonly PropertyKey<LightShadowMapTexture> Key = new PropertyKey<LightShadowMapTexture>("LightShadowMapTexture.Key", typeof(LightShadowMapTexture));

        /// <summary>
        /// Initializes a new instance of the <see cref="LightShadowMapTexture" /> struct.
        /// </summary>
        /// <param name="lightComponent">The light component.</param>
        /// <param name="light">The light component.</param>
        /// <param name="shadowMap">The light.</param>
        /// <param name="size">The shadow map.</param>
        /// <param name="renderer">The renderer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// lightComponent
        /// or
        /// light
        /// or
        /// shadowMap
        /// or
        /// renderer
        /// </exception>
        public LightShadowMapTexture()
        {
            CascadeSplits = new Vector4();
            WorldToShadowCascadeUV = new Matrix[4];
        }

        public LightComponent LightComponent { get; private set; }

        public IDirectLight Light { get; private set; }

        public LightShadowMap Shadow { get; private set; }

        public Type FilterType { get; private set; }

        public int Size { get; private set; }

        public int CascadeCount { get; set; }

        public ShadowMapAtlasTexture Atlas { get; internal set; }

        public ILightShadowMapRenderer Renderer;

        public Vector4 CascadeSplits;

        public readonly Matrix[] WorldToShadowCascadeUV;

        public void Initialize(LightComponent lightComponent, IDirectLight light, LightShadowMap shadowMap, int size, ILightShadowMapRenderer renderer)
        {
            if (lightComponent == null) throw new ArgumentNullException("lightComponent");
            if (light == null) throw new ArgumentNullException("light");
            if (shadowMap == null) throw new ArgumentNullException("shadowMap");
            if (renderer == null) throw new ArgumentNullException("renderer");
            LightComponent = lightComponent;
            Light = light;
            Shadow = shadowMap;
            Size = size;
            FilterType = Shadow.Filter == null || !Shadow.Filter.RequiresCustomBuffer() ? null : Shadow.Filter.GetType();
            Renderer = renderer;
            lightComponent.Tags.Set(Key, this);
        }

        public static LightShadowMapTexture GetFromLightComponent(LightComponent lightComponent)
        {
            if (lightComponent == null) throw new ArgumentNullException("lightComponent");
            return lightComponent.Tags.Get(Key);
        }

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