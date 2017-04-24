// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ScriptShader.Effects;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public struct LightBinding
    {
        public LightBinding(Light light) : this()
        {
            Light = light;
            LightShaderType = light.LightShaderType;
        }

        /// <summary>
        /// The light.
        /// </summary>
        public Light Light;

        /// <summary>
        /// Specifies LightShaderType. Might be different than Light's one, since depending on Material or HW limitations it might be downgraded or ignored.
        /// </summary>
        public LightShaderType LightShaderType;
    }

    public class LightingPermutation : Permutation
    {
        public static ParameterKey<LightingPermutation> Key = ParameterKeys.Resource(new LightingPermutation(new LightBinding[0]));

        public LightingPermutation(IEnumerable<LightBinding> lightBindings)
        {
            PerPixelDiffuseDirectionalLights = lightBindings.Where(IsDirectionalDiffusePixel).ToArray();
            PerPixelDirectionalLights = lightBindings.Where(IsDirectionalDiffuseSpecularPixel).ToArray();
            PerVertexDirectionalLights = lightBindings.Where(IsDirectionalDiffuseVertex).ToArray();
            PerVertexDiffusePixelSpecularDirectionalLights = lightBindings.Where(IsDirectionalDiffuseVertexPixelSpecular).ToArray();
        }

        public LightBinding[] PerPixelDiffuseDirectionalLights { get; set; }
        public LightBinding[] PerPixelDirectionalLights { get; set; }
        public LightBinding[] PerVertexDirectionalLights { get; set; }
        public LightBinding[] PerVertexDiffusePixelSpecularDirectionalLights { get; set; }

        public override object GenerateKey()
        {
            return new KeyInfo
                {
                    PerPixelDiffuseDirectionalLightCount = PerPixelDiffuseDirectionalLights.Length,
                    PerPixelDirectionalLightCount = PerPixelDirectionalLights.Length,
                    PerVertexDirectionalLightCount = PerVertexDirectionalLights.Length,
                    PerVertexDiffusePixelSpecularDirectionalLightCount = PerVertexDiffusePixelSpecularDirectionalLights.Length,
                };
        }

        public static bool IsDirectionalDiffuseVertexPixelSpecular(LightBinding x)
        {
            return (x.LightShaderType & LightShaderType.DiffuseVertexSpecularPixel) == LightShaderType.DiffuseVertexSpecularPixel;
        }

        public static bool IsDirectionalDiffuseVertex(LightBinding x)
        {
            return (x.LightShaderType & LightShaderType.DiffuseVertex) == LightShaderType.DiffuseVertex;
        }

        public static bool IsDirectionalDiffusePixel(LightBinding x)
        {
            return (x.LightShaderType == LightShaderType.DiffusePixel);
        }

        public static bool IsDirectionalDiffuseSpecularPixel(LightBinding x)
        {
            return (x.LightShaderType & LightShaderType.DiffuseSpecularPixel) == LightShaderType.DiffuseSpecularPixel;
        }

        public class KeyInfo : IEquatable<KeyInfo>
        {
            public int PerPixelDiffuseDirectionalLightCount;
            public int PerPixelDirectionalLightCount;
            public int PerVertexDirectionalLightCount;
            public int PerVertexDiffusePixelSpecularDirectionalLightCount;

            public bool Equals(KeyInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return PerPixelDiffuseDirectionalLightCount == other.PerPixelDiffuseDirectionalLightCount
                    && PerPixelDirectionalLightCount == other.PerPixelDirectionalLightCount
                    && PerVertexDirectionalLightCount == other.PerVertexDirectionalLightCount
                    && PerVertexDiffusePixelSpecularDirectionalLightCount == other.PerVertexDiffusePixelSpecularDirectionalLightCount;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((KeyInfo)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = PerPixelDiffuseDirectionalLightCount;
                    hashCode = (hashCode * 397) ^ PerPixelDirectionalLightCount;
                    hashCode = (hashCode * 397) ^ PerVertexDirectionalLightCount;
                    hashCode = (hashCode * 397) ^ PerVertexDiffusePixelSpecularDirectionalLightCount;
                    return hashCode;
                }
            }
        }
    }
}
