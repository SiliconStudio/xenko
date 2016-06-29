// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public interface ILightShadowMapShaderData
    {
    }

    public interface ILightShadowMapShaderGroupData
    {
        void ApplyShader(ShaderMixinSource mixin);

        void UpdateLayout(string compositionName);
        void UpdateLightCount(int lightLastCount, int lightCurrentCount);

        void ApplyViewParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights);

        void ApplyDrawParameters(RenderDrawContext context, ParameterCollection parameters, FastListStruct<LightDynamicEntry> currentLights, ref BoundingBoxExt boundingBox);
    }

    [Flags]
    public enum LightShadowType : ushort // DO NOT CHANGE the size of this type. It is used to caculate the shaderKeyId in LightComponentForwardRenderer. 
    {
        Cascade1 = 0x1,
        Cascade2 = 0x2,
        Cascade4 = 0x3,
        
        CascadeMask = 0x3,

        Debug = 0x4,

        BlendCascade = 0x8,

        DepthRangeAuto = 0x10,

        FilterMask = 0xF00,

        PCF3x3 = 0x100,

        PCF5x5 = 0x200,

        PCF7x7 = 0x300
    }

    /// <summary>
    /// An allocated shadow map texture associated to a light.
    /// </summary>
    public class LightShadowMapTexture
    {
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
        }

        public LightComponent LightComponent { get; private set; }

        public IDirectLight Light { get; private set; }

        public LightShadowMap Shadow { get; private set; }

        public Type FilterType { get; private set; }

        public byte TextureId { get; internal set; }

        public LightShadowType ShadowType { get; internal set; }

        public int Size { get; private set; }

        public int CascadeCount { get; set; }

        public ShadowMapAtlasTexture Atlas { get; internal set; }

        public ILightShadowMapRenderer Renderer;

        public ILightShadowMapShaderData ShaderData;

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
            Atlas = null; // Reset the atlas, It will be setup after

            ShadowType = renderer.GetShadowType(Shadow);
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

        // Even if C# things Rectangle1, Rectangle2 and Rectangle3 are not used,
        // they are indirectly in `GetRectangle' and `SetRectangle' through pointer
        // arithmetics.
        private Rectangle Rectangle0;
#pragma warning disable 169
        private Rectangle Rectangle1;
        private Rectangle Rectangle2;
        private Rectangle Rectangle3;
#pragma warning restore 169

    }
}
