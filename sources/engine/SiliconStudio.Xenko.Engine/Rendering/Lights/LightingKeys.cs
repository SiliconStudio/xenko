// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Keys used for lighting.
    /// </summary>
    [DataContract]
    public partial class LightingKeys : ShaderMixinParameters
    {
        public static readonly ParameterKey<List<ShaderSource>> DirectLightGroups = ParameterKeys.New((List<ShaderSource>)null);

        public static readonly ParameterKey<List<ShaderSource>> EnvironmentLights = ParameterKeys.New((List<ShaderSource>)null);
       
        private static readonly List<ShaderSource> DefaultAmbientLighting = new List<ShaderSource>() { new ShaderClassSource("LightSimpleAmbient"), };

        public static void EnableFixedAmbientLight(NextGenParameterCollection parameters, bool enable)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            if (enable)
            {
                parameters.SetResourceSlow(EnvironmentLights, DefaultAmbientLighting);
            }
            else
            {
                parameters.Remove(EnvironmentLights);
            }
        }
    }
}
