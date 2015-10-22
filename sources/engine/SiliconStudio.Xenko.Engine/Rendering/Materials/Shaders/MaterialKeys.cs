// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    public class MaterialKeys
    {
        public static readonly ParameterKey<ShaderSource> VertexStageSurfaceShaders = ParameterKeys.New<ShaderSource>();
        public static readonly ParameterKey<ShaderSource> DomainStageSurfaceShaders = ParameterKeys.New<ShaderSource>();
        public static readonly ParameterKey<ShaderSource> PixelStageSurfaceShaders = ParameterKeys.New<ShaderSource>();
        
        public static readonly ParameterKey<ShaderSource> VertexStageStreamInitializer = ParameterKeys.New<ShaderSource>();
        public static readonly ParameterKey<ShaderSource> DomainStageStreamInitializer = ParameterKeys.New<ShaderSource>();
        public static readonly ParameterKey<ShaderSource> PixelStageStreamInitializer = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> TessellationShader = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<ShaderSource> PixelStageSurfaceFilter = ParameterKeys.New<ShaderSource>();

        public static readonly ParameterKey<Texture> BlendMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> BlendValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> DisplacementMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> DisplacementValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> DisplacementIntensityMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> DisplacementIntensityValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> NormalMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Vector3> NormalValue = ParameterKeys.New<Vector3>();

        public static readonly ParameterKey<Texture> DiffuseMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color4> DiffuseValue = ParameterKeys.New<Color4>();

        public static readonly ParameterKey<Texture> DiffuseSpecularAlphaBlendMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> DiffuseSpecularAlphaBlendValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> AlphaBlendColorMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color3> AlphaBlendColorValue = ParameterKeys.New<Color3>();

        public static readonly ParameterKey<Texture> AlphaDiscardMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> AlphaDiscardValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> SpecularMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color3> SpecularValue = ParameterKeys.New<Color3>();
        public static readonly ParameterKey<float> SpecularIntensityValue = ParameterKeys.New<float>();
        
        public static readonly ParameterKey<Texture> GlossinessMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> GlossinessValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> AmbientOcclusionMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> AmbientOcclusionValue = ParameterKeys.New<float>();
        public static readonly ParameterKey<float> AmbientOcclusionDirectLightingFactorValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> CavityMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> CavityValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<float> CavityDiffuseValue = ParameterKeys.New<float>();
        public static readonly ParameterKey<float> CavitySpecularValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> MetalnessMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> MetalnessValue = ParameterKeys.New<float>();

        public static readonly ParameterKey<Texture> EmissiveMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<Color4> EmissiveValue = ParameterKeys.New<Color4>();

        public static readonly ParameterKey<Texture> EmissiveIntensityMap = ParameterKeys.New<Texture>();
        public static readonly ParameterKey<float> EmissiveIntensity = ParameterKeys.New<float>();

        /// <summary>
        /// Generic texture key used by a material
        /// </summary>
        public static readonly ParameterKey<Texture> GenericTexture = ParameterKeys.New<Texture>();

        /// <summary>
        /// Generic texture key used by a material
        /// </summary>
        public static readonly ParameterKey<Color3> GenericValueColor3 = ParameterKeys.New<Color3>();

        /// <summary>
        /// Generic texture key used by a material
        /// </summary>
        public static readonly ParameterKey<Color4> GenericValueColor4 = ParameterKeys.New<Color4>();

        /// <summary>
        /// Generic texture key used by a material
        /// </summary>
        public static readonly ParameterKey<Vector4> GenericValueVector4 = ParameterKeys.New<Vector4>();

        /// <summary>
        /// Texture UV scaling
        /// </summary>
        public static readonly ParameterKey<Vector2> TextureScale = ParameterKeys.New<Vector2>(Vector2.One);

        /// <summary>
        /// Texture UV offset
        /// </summary>
        public static readonly ParameterKey<Vector2> TextureOffset = ParameterKeys.New<Vector2>();

        /// <summary>
        /// Generic texture key used by a material
        /// </summary>
        public static readonly ParameterKey<float> GenericValueFloat = ParameterKeys.New<float>();

        /// <summary>
        /// Generic sampler key used by a material
        /// </summary>
        public static readonly ParameterKey<SamplerState> Sampler = ParameterKeys.New<SamplerState>();

        public static readonly ParameterKey<bool> HasSkinningPosition = ParameterKeys.New<bool>();

        public static readonly ParameterKey<bool> HasSkinningNormal = ParameterKeys.New<bool>();

        public static readonly ParameterKey<bool> HasNormalMap = ParameterKeys.New<bool>();

        public static readonly ParameterKey<bool> HasSkinningTangent = ParameterKeys.New<bool>();

        public static readonly ParameterKey<bool> HasTransparency = ParameterKeys.New<bool>();

        public static readonly ParameterKey<int> SkinningMaxBones = ParameterKeys.New<int>(56);

        public static readonly ParameterKey<int> SkinningBones = ParameterKeys.New<int>(0);

        static MaterialKeys()
        {
            //SpecularPowerScaled = ParameterKeys.NewDynamic(ParameterDynamicValue.New<float, float>(SpecularPower, ScaleSpecularPower));
        }

        private static void ScaleSpecularPower(ref float specularPower, ref float scaledSpecularPower)
        {
            scaledSpecularPower = (float)Math.Pow(2.0f, 1.0f + specularPower * 13.0f);
        }
    }
}