// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Keys used for lighting.
    /// </summary>
    [DataContract]
    public partial class LightingKeys : ShaderMixinParameters
    {
        public static readonly ParameterKey<ShaderSource[]> DirectLightGroups = ParameterKeys.New((ShaderSource[])null);

        public static readonly ParameterKey<ShaderSource[]> EnvironmentLights = ParameterKeys.New((ShaderSource[])null);

        public static readonly ParameterKey<bool> ReceiveShadows = ParameterKeys.New(true);

        public static readonly ParameterKey<bool> CastShadows = ParameterKeys.New(true);

        private static readonly ShaderSource[] DefaultAmbientLighting = new ShaderSource[] { new ShaderClassSource("LightSimpleAmbient"), };

        public static void EnableFixedAmbientLight(ParameterCollection parameters, bool enable)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            if (enable)
            {
                parameters.Set(EnvironmentLights, DefaultAmbientLighting);
            }
            else
            {
                parameters.Remove(EnvironmentLights);
            }
        }
    }
}
