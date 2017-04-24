// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Xenko.DataModel;
using SiliconStudio.Xenko.Rendering.Data;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    public class MaterialShaderPlugin : ShaderPlugin<RenderPassPlugin>
    {
        public MaterialShaderPlugin()
        {
            Flags |= EffectPluginFlags.StaticPermutation;
        }

        public object GenerateEffectKey(Dictionary<ParameterKey, object> permutations)
        {
            object permutation;
            if (permutations.TryGetValue(EffectMeshDataPermutation.Key, out permutation))
            {
                return new MaterialShaderPluginPermutationKey((Mesh)permutation);
            }

            return null;
        }

        public override void SetupShaders(EffectMesh effectMesh)
        {
            var permutationKey = new MaterialShaderPluginPermutationKey(effectMesh.MeshData);

            // Determines the max number of bones from the currect graphics profile
            int defaultSkinningMaxBones = GraphicsDevice.Features.Profile <= GraphicsProfile.Level_9_3 ? 56 : 200;

            if (permutationKey.SkinningPosition)
            {
                DefaultShaderPass.Macros.Add(new ShaderMacro("SkinningMaxBones", defaultSkinningMaxBones));
                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, new ShaderClassSource("TransformationSkinning"), true);
            }

            if (permutationKey.SkinningNormal)
                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, new ShaderClassSource("NormalSkinning"), true);
            if (permutationKey.SkinningTangent)
                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, new ShaderClassSource("TangentSkinning"), true);

            if (permutationKey.AlbedoMaterial != null)
                BasicShaderPlugin.ApplyMixinClass(DefaultShaderPass.Shader, permutationKey.AlbedoMaterial, true);
        }

        public override void SetupResources(EffectMesh effectMesh)
        {
            if (effectMesh.MeshData.Material.Parameters.Get(EffectData.NeedAlphaBlending))
            {
                effectMesh.Parameters.Set(BlendStateKey, GraphicsDevice.BlendStates.NonPremultiplied);
            }
        }

        private class MaterialShaderPluginPermutationKey
        {
            public MaterialShaderPluginPermutationKey(Mesh meshData)
            {
                if (meshData.Draw == null)
                    return;

                var subMeshData = meshData.Draw; //Data[Mesh.StandardSubMeshData];

                NeedAlphaBlending = meshData.EffectParameters.Get(EffectData.NeedAlphaBlending);
                AlbedoMaterial = meshData.EffectParameters.Get(EffectData.AlbedoMaterial);

                var vertexElementUsages = new HashSet<string>(subMeshData
                    .VertexBuffers
                    .SelectMany(x => x.Declaration.VertexElements)
                    .Select(x => x.SemanticAsText));

                if (vertexElementUsages.Contains("BLENDWEIGHT"))
                {
                    SkinningPosition = true;
                    if (vertexElementUsages.Contains("NORMAL"))
                        SkinningNormal = true;
                    if (vertexElementUsages.Contains("TANGENT"))
                        SkinningTangent = true;
                }
            }

            public ShaderMixinSource AlbedoMaterial { get; set; }
            public bool NeedAlphaBlending { get; set; }
            public bool SkinningPosition { get; set; }
            public bool SkinningNormal { get; set; }
            public bool SkinningTangent { get; set; }

            protected bool Equals(MaterialShaderPluginPermutationKey other)
            {
                return ShaderSourceComparer.Default.Equals(AlbedoMaterial, other.AlbedoMaterial)
                    && SkinningPosition.Equals(other.SkinningPosition)
                    && SkinningNormal.Equals(other.SkinningNormal)
                    && SkinningTangent.Equals(other.SkinningTangent)
                    && NeedAlphaBlending.Equals(other.NeedAlphaBlending);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(MaterialShaderPluginPermutationKey)) return false;
                return Equals((MaterialShaderPluginPermutationKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (AlbedoMaterial != null ? ShaderSourceComparer.Default.GetHashCode(AlbedoMaterial) : 0);
                    hashCode = (hashCode * 397) ^ SkinningPosition.GetHashCode();
                    hashCode = (hashCode * 397) ^ SkinningNormal.GetHashCode();
                    hashCode = (hashCode * 397) ^ SkinningTangent.GetHashCode();
                    hashCode = (hashCode * 397) ^ NeedAlphaBlending.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
