// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class LightingShaderPlugin : ShaderPlugin<LightingPlugin>
    {
        private ShaderMixinSource mainShaderSource;
        private ShaderMacro[] mainShaderMacros;

        public RenderTargetsPlugin MainTargetPlugin { get; set; }

        public override void SetupPasses(EffectMesh effectMesh)
        {
            var effectSystem = Services.GetSafeServiceAs<IEffectSystemOld>();

            throw new NotImplementedException();
            EffectShaderPass mainShaderPass;
            //DefaultShaderPass = FindShaderPassFromPlugin(MainTargetPlugin);

            //effectSystemOld.RegisterPermutation(DefaultShaderPass, this, ShadowMapPermutationArray.Key, SetupShadersPermutationReceiver);
            //effectSystemOld.RegisterPermutation(DefaultShaderPass, this, ShadowMapPermutationArray.Key, SetupShadersPermutationCaster);
            //effectSystemOld.RegisterPermutation(DefaultShaderPass, this, LightingPermutation.Key, SetupLighting);
        }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            mainShaderSource = (ShaderMixinSource)DefaultShaderPass.Shader.Clone();
            mainShaderMacros = DefaultShaderPass.Macros.ToArray();
        }

        public void SetupLighting(EffectShaderPass effectPass, object permutationKeyObject)
        {
            var permutationKey = (LightingPermutation.KeyInfo)permutationKeyObject;

            if (permutationKey.PerPixelDirectionalLightCount > 0)
            {
                effectPass.Shader.Mixins.Add(new ShaderClassSource("LightMultiDirectionalShadingPerPixel", permutationKey.PerPixelDirectionalLightCount));

                // Light colors
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingPerPixelKeys.LightColorsWithGamma, ParameterDynamicValue.New(LightingPermutation.Key,
                    (ref LightingPermutation lightPermutation, ref Color3[] lightColorsWithGamma) =>
                        LightColorsUpdate(lightPermutation.PerPixelDirectionalLights, lightColorsWithGamma)
                    , autoCheckDependencies: false));

                // LightDirectionVS
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingPerPixelKeys.LightDirectionsVS, ParameterDynamicValue.New(LightingPermutation.Key, TransformationKeys.View, (ref LightingPermutation lightPermutation, ref Matrix view, ref Vector3[] lightDirectionVS) =>
                    {
                        int index = 0;
                        foreach (var lightBinding in lightPermutation.PerPixelDirectionalLights)
                        {
                            var lightDirection = ((DirectionalLight)lightBinding.Light).LightDirection;
                            LightKeys.LightDirectionVSUpdate(ref lightDirection, ref view, ref lightDirectionVS[index++]);
                        }
                    }, autoCheckDependencies: false));
            }

            if (permutationKey.PerPixelDiffuseDirectionalLightCount > 0)
            {
                effectPass.Shader.Mixins.Add(new ShaderClassSource("LightMultiDirectionalShadingDiffusePerPixel", permutationKey.PerPixelDiffuseDirectionalLightCount));

                // Light colors
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingDiffusePerPixelKeys.LightColorsWithGamma, ParameterDynamicValue.New(LightingPermutation.Key,
                    (ref LightingPermutation lightPermutation, ref Color3[] lightColorsWithGamma) =>
                        LightColorsUpdate(lightPermutation.PerPixelDiffuseDirectionalLights, lightColorsWithGamma)
                    , autoCheckDependencies: false));

                // LightDirectionVS
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingDiffusePerPixelKeys.LightDirectionsVS, ParameterDynamicValue.New(LightingPermutation.Key, TransformationKeys.View, (ref LightingPermutation lightPermutation, ref Matrix view, ref Vector3[] lightDirectionVS) =>
                {
                    int index = 0;
                    foreach (var lightBinding in lightPermutation.PerPixelDiffuseDirectionalLights)
                    {
                        var lightDirection = ((DirectionalLight)lightBinding.Light).LightDirection;
                        LightKeys.LightDirectionVSUpdate(ref lightDirection, ref view, ref lightDirectionVS[index++]);
                    }
                }, autoCheckDependencies: false));
            } 
            
            if (permutationKey.PerVertexDirectionalLightCount > 0)
            {
                effectPass.Shader.Mixins.Add(new ShaderClassSource("LightMultiDirectionalShadingPerVertex", permutationKey.PerVertexDirectionalLightCount));

                // Light colors
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingPerVertexKeys.LightColorsWithGamma, ParameterDynamicValue.New(LightingPermutation.Key,
                    (ref LightingPermutation lightPermutation, ref Color3[] lightColorsWithGamma) =>
                        LightColorsUpdate(lightPermutation.PerVertexDirectionalLights, lightColorsWithGamma)
                    , autoCheckDependencies: false));

                // LightDirectionWS
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingPerVertexKeys.LightDirectionsWS, ParameterDynamicValue.New(LightingPermutation.Key, (ref LightingPermutation lightPermutation, ref Vector3[] lightDirectionWS) =>
                    {
                        int index = 0;
                        foreach (var lightBinding in lightPermutation.PerVertexDirectionalLights)
                        {
                            lightDirectionWS[index++] = ((DirectionalLight)lightBinding.Light).LightDirection;
                        }
                    }, autoCheckDependencies: false));
            }

            if (permutationKey.PerVertexDiffusePixelSpecularDirectionalLightCount > 0)
            {
                effectPass.Shader.Mixins.Add(new ShaderClassSource("LightMultiDirectionalShadingSpecularPerPixel", permutationKey.PerVertexDiffusePixelSpecularDirectionalLightCount));

                // Light colors
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingSpecularPerPixelKeys.LightColorsWithGamma, ParameterDynamicValue.New(LightingPermutation.Key,
                    (ref LightingPermutation lightPermutation, ref Color3[] lightColorsWithGamma) =>
                        LightColorsUpdate(lightPermutation.PerVertexDiffusePixelSpecularDirectionalLights, lightColorsWithGamma)
                    , autoCheckDependencies: false));

                // LightDirectionWS
                effectPass.Parameters.AddDynamic(LightMultiDirectionalShadingSpecularPerPixelKeys.LightDirectionsWS, ParameterDynamicValue.New(LightingPermutation.Key, (ref LightingPermutation lightPermutation, ref Vector3[] lightDirectionWS) =>
                    {
                        int index = 0;
                        foreach (var lightBinding in lightPermutation.PerVertexDiffusePixelSpecularDirectionalLights)
                        {
                            lightDirectionWS[index++] = ((DirectionalLight)lightBinding.Light).LightDirection;
                        }
                    }, autoCheckDependencies: false));
            }
        }

        private static void LightColorsUpdate(LightBinding[] lightBindings, Color3[] lightColorsWithGamma)
        {
            int index = 0;
            foreach (var lightBinding in lightBindings)
            {
                var lightColor = lightBinding.Light.LightColor;
                lightColor.Pow(Color.DefaultGamma);
                lightColorsWithGamma[index++] = lightColor;
            }
        }

        public IEnumerable<EffectShaderPass> SetupShadersPermutationCaster(object permutationKey)
        {
            var shadowMapPermutation = (ShadowMap)permutationKey;

            var shaderCaster = new ShaderClassSource("ShadowMapCasterBase");

            for (int i = 0; i < shadowMapPermutation.LevelCount; ++i)
            {
                // Shadow caster
                var shaderPass = new EffectShaderPass(shadowMapPermutation.Passes[i], "ShadowMap");
                shaderPass.Parameters.AddSources(shadowMapPermutation.Passes[i].Parameters);
                shaderPass.Shader = (ShaderMixinSource)mainShaderSource.Clone();
                shaderPass.Macros.AddRange(mainShaderMacros);
                shaderPass.Shader.Mixins.Add(shaderCaster);
                yield return shaderPass;
            }
        }

        public void SetupShadersPermutationReceiver(EffectShaderPass effectPass, object permutationKey)
        {
            var currentShadowMapsPermutation = (ShadowMapPermutationArray)permutationKey;

            if (currentShadowMapsPermutation.ShadowMaps.Count == 0)
                return;

            var shadowsArray = new ShaderArraySource();
            effectPass.Shader.Mixins.Add("ShadowMapReceiver");
            effectPass.Shader.Compositions.Add("shadows", shadowsArray);

            // Group by shadow map types (one group can be processed in the same loop).
            // Currently based on filtering type and number of cascades (but some more parameters might affect this later as new type of shadow maps are introduced).
            var shadowMapTypes = currentShadowMapsPermutation.ShadowMaps.GroupBy(x => Tuple.Create(x.ShadowMap.Filter.GetType(), x.ShadowMap.LevelCount, x.ShadowMap.Texture));

            int shadowMapTypeIndex = 0;
            foreach (var shadowMapType in shadowMapTypes)
            {
                var shadowMapTypeCopy = shadowMapType.ToArray();
                var shadowMixin = new ShaderMixinSource();

                // Currently use shadow map count, but we should probably use next power of two to limit number of permutations.
                var maxShadowMapCount = shadowMapTypeCopy.Length;

                // Setup shadow mapping
                shadowMixin.Mixins.Add(new ShaderClassSource("ShadowMapCascadeBase", shadowMapType.Key.Item2, 0, maxShadowMapCount, 1));
                //shadowMixin.Mixins.Add(new ShaderClassSource("LightDirectionalShading", shadowMapPermutation.Index));

                // Setup the filter
                // TODO: Use static based on type of filter? (should use Key instead of First()).
                shadowMixin.Mixins.Add(shadowMapType.First().ShadowMap.Filter.GenerateShaderSource(maxShadowMapCount));

                shadowsArray.Add(shadowMixin);

                // Register keys for this shadow map type
                var shadowSubKey = string.Format(".shadows[{0}]", shadowMapTypeIndex++);

                effectPass.Parameters.Set(ShadowMapKeys.Texture.AppendKey(shadowSubKey), shadowMapType.Key.Item3);

                effectPass.Parameters.Set(LightingPlugin.ShadowMapLightCount.AppendKey(shadowSubKey), shadowMapTypeCopy.Length);

                effectPass.Parameters.AddDynamic(LightingPlugin.ReceiverInfo.AppendKey(shadowSubKey), ParameterDynamicValue.New(ShadowMapPermutationArray.Key, TransformationKeys.World, TransformationKeys.ViewProjection, (ref ShadowMapPermutationArray shadowMapPermutations, ref Matrix world, ref Matrix viewProj, ref ShadowMapReceiverInfo[] output) =>
                    {
                        unsafe
                        {
                            for (int i = 0; i < shadowMapTypeCopy.Length; ++i)
                            {
                                var permutationParameters = shadowMapTypeCopy[i].ShadowMap.Parameters;

                                // TODO: Optimize dictionary access this using SetKeyMapping
                                var shadowMapData = permutationParameters.Get(LightingPlugin.ViewProjectionArray);
                                var textureCoords = permutationParameters.Get(LightingPlugin.CascadeTextureCoordsBorder);
                                var distanceMax = permutationParameters.Get(ShadowMapKeys.DistanceMax);
                                var lightDirection = permutationParameters.Get(LightKeys.LightDirection);

                                Matrix* vpPtr = &shadowMapData.ViewProjReceiver0;
                                fixed (Matrix* wvpPtr = &output[i].WorldViewProjReceiver0)
                                {
                                    for (int j = 0; j < 4; ++j)
                                        Matrix.Multiply(ref world, ref vpPtr[j], ref wvpPtr[j]);
                                }

                                output[i].Offset0 = shadowMapData.Offset0;
                                output[i].Offset1 = shadowMapData.Offset1;
                                output[i].Offset2 = shadowMapData.Offset2;
                                output[i].Offset3 = shadowMapData.Offset3;

                                fixed (Vector4* targetPtr = &output[i].CascadeTextureCoordsBorder0)
                                fixed (Vector4* sourcePtr = &textureCoords[0])
                                {
                                    for (int j = 0; j < 4; ++j)
                                        targetPtr[j] = sourcePtr[j];
                                }

                                output[i].ShadowLightDirection = lightDirection;
                                LightKeys.LightDirectionVSUpdate(ref output[i].ShadowLightDirection, ref viewProj, ref output[i].ShadowLightDirectionVS);
                                output[i].ShadowMapDistance = distanceMax;
                                output[i].ShadowLightColor = shadowMapData.LightColor;
                            }
                        }
                    }, autoCheckDependencies: false)); // Always update (permutation won't get updated, only its content)!

                // Register VSM filter if necessary
                if (shadowMapType.Key.Item1 == typeof(ShadowMapFilterVsm))
                {
                    effectPass.Parameters.AddDynamic(LightingPlugin.ReceiverVsmInfo.AppendKey(shadowSubKey), ParameterDynamicValue.New(ShadowMapPermutationArray.Key, (ref ShadowMapPermutationArray shadowMapPermutations, ref ShadowMapReceiverVsmInfo[] output) =>
                    {
                        for (int i = 0; i < shadowMapTypeCopy.Length; ++i)
                        {
                            var permutationParameters = shadowMapTypeCopy[i].ShadowMap.Parameters;
                            output[i].BleedingFactor = permutationParameters.Get(ShadowMapFilterVsm.VsmBleedingFactor);
                            output[i].MinVariance = permutationParameters.Get(ShadowMapFilterVsm.VsmMinVariance);
                        }
                    }, autoCheckDependencies: false));
                }
            }
        }
    }
}
