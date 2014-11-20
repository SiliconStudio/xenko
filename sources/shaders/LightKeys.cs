// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Keys used for Lighting plugin.
    /// </summary>
    public static class LightKeys
    {
        internal static readonly ParameterKey<int> ConfigurationIndex = ParameterKeys.New(-1);

        /// <summary>
        /// Diffuse color.
        /// </summary>
        public static readonly ParameterKey<Color3> LightColor = ParameterKeys.New(new Color3(1.0f, 1.0f, 1.0f));

        /// <summary>
        /// Light radius for point light.
        /// </summary>
        public static readonly ParameterKey<float> LightRadius = ParameterKeys.New(50.0f);

        /// <summary>
        /// Light intensity..
        /// </summary>
        public static readonly ParameterKey<float> LightIntensity = ParameterKeys.New(1.0f);

        /// <summary>
        /// Light cutoff for point light.
        /// </summary>
        public static readonly ParameterKey<float> LightAttenuationCutoff = ParameterKeys.New(0.1f);

        /// <summary>
        /// Light position.
        /// </summary>
        public static readonly ParameterKey<Vector3> LightPosition = ParameterKeys.New(Vector3.Zero);

        /// <summary>
        /// Light direction.
        /// </summary>
        public static readonly ParameterKey<Vector3> LightDirection = ParameterKeys.New(new Vector3(-1.0f, -1.0f, -1.0f));

        /// <summary>
        /// Light direction VS.
        /// </summary>
        public static readonly ParameterKey<Vector3> LightDirectionVS = ParameterKeys.NewDynamic(ParameterDynamicValue.New<Vector3, Vector3, Matrix>(LightDirection, TransformationKeys.WorldView, LightDirectionVSUpdate));

        public static void LightDirectionVSUpdate(ref Vector3 lightDirection, ref Matrix viewProj, ref Vector3 lightDirectionVS)
        {
            var temp = Vector3.TransformNormal(lightDirection, viewProj);
            temp.Normalize();
            lightDirectionVS = temp;
        }
    }
}