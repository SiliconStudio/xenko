// Copyright (c) 2011 Silicon Studio

using System.Linq;
using Xenko.Framework.Mathematics;

namespace Xenko.Effects.Modules
{
    /// <summary>
    /// Keys used for Lighting plugin.
    /// </summary>
    public static class LightKeys
    {
        /// <summary>
        /// Light position.
        /// </summary>
        public static readonly ParameterResourceKey<Effect> LightKey = ParameterKeys.Resource<Effect>();

        /// <summary>
        /// Diffuse color.
        /// </summary>
        public static readonly ParameterValueKey<Color3> LightColor = ParameterKeys.Value(new Color3(1.0f, 1.0f, 1.0f));

        /// <summary>
        /// Light radius for point light.
        /// </summary>
        public static readonly ParameterValueKey<float> LightRadius = ParameterKeys.Value(50.0f);

        /// <summary>
        /// Light intensity..
        /// </summary>
        public static readonly ParameterValueKey<float> LightIntensity = ParameterKeys.Value(1.0f);

        /// <summary>
        /// Light cutoff for point light.
        /// </summary>
        public static readonly ParameterValueKey<float> LightAttenuationCutoff = ParameterKeys.Value(0.1f);

        /// <summary>
        /// Light position.
        /// </summary>
        public static readonly ParameterValueKey<Vector3> LightPosition = ParameterKeys.Value(Vector3.Zero);

        /// <summary>
        /// Light direction.
        /// </summary>
        public static readonly ParameterValueKey<Vector3> LightDirection = ParameterKeys.Value(new Vector3(-1.0f, -1.0f, -1.0f));

        /// <summary>
        /// Light direction VS.
        /// </summary>
        public static readonly ParameterValueKey<Vector3> LightDirectionVS = ParameterKeys.Value(ParameterDynamicValue.New<Vector3, Vector3, Matrix>(LightDirection, TransformationKeys.WorldView, LightDirectionVSUpdate));

        public static void LightDirectionVSUpdate(ref Vector3 lightDirection, ref Matrix viewProj, ref Vector3 lightDirectionVS)
        {
            var temp = Vector3.TransformNormal(lightDirection, viewProj);
            temp.Normalize();
            lightDirectionVS = temp;
        }
    }
}