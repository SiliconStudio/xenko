// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    public class TessellationPlugin : ShaderPlugin<RenderPassPlugin>
    {
        public TessellationPlugin()
        {
            Flags |= EffectPluginFlags.StaticPermutation;
        }

        public object GenerateEffectKey(Dictionary<ParameterKey, object> permutations)
        {
            object permutation;
            if (permutations.TryGetValue(EffectMeshDataPermutation.Key, out permutation))
            {
                return new TessellationPluginPermutationKey((Mesh)permutation);
            }

            return null;
        }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            var permutationKey = new TessellationPluginPermutationKey(effectMesh.MeshData);
            if (permutationKey != null && permutationKey.Tessellation != null)
            {
                int inputControlPointCount = 3;

                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, permutationKey.Tessellation, true);

                // Apply Displacement AEN plugin if both AEN is available and displacement is active
                if (permutationKey.TessellationAEN)
                {
                    BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, new ShaderClassSource("TessellationDisplacementAEN"), true);
                    DefaultShaderPass.SubMeshDataKey = "TessellationAEN";
                    inputControlPointCount = 12;
                }

                DefaultShaderPass.Macros.Add(new ShaderMacro("InputControlPointCount", inputControlPointCount));
                DefaultShaderPass.Macros.Add(new ShaderMacro("OutputControlPointCount", 3));
            }
        }

        private class TessellationPluginPermutationKey
        {
            public TessellationPluginPermutationKey(Mesh meshData)
            {
                if (meshData.Draw == null)
                    return;

                Tessellation = meshData.EffectParameters.Get(EffectData.Tessellation);
                if (Tessellation != null)
                {
                    TessellationAEN = meshData.Material.Parameters.Get(EffectData.TessellationAEN);
                }
            }

            public ShaderMixinSource Tessellation { get; set; }
            public bool TessellationAEN { get; set; }

            protected bool Equals(TessellationPluginPermutationKey other)
            {
                return ShaderSourceComparer.Default.Equals(Tessellation, other.Tessellation)
                    && TessellationAEN.Equals(other.TessellationAEN);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(TessellationPluginPermutationKey)) return false;
                return Equals((TessellationPluginPermutationKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Tessellation != null ? ShaderSourceComparer.Default.GetHashCode(Tessellation) : 0);
                    hashCode = (hashCode * 397) ^ TessellationAEN.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
