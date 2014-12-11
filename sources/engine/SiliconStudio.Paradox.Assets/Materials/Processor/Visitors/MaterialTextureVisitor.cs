// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialTextureVisitor : MaterialBaseVisitor
    {
        #region Private static members

        /// <summary>
        /// Available default texture keys.
        /// </summary>
        private static readonly List<ParameterKey<Graphics.Texture>> DefaultTextureKeys = new List<ParameterKey<Graphics.Texture>>()
            {
                TexturingKeys.Texture0,
                TexturingKeys.Texture1,
                TexturingKeys.Texture2,
                TexturingKeys.Texture3,
                TexturingKeys.Texture4,
                TexturingKeys.Texture5,
                TexturingKeys.Texture6,
                TexturingKeys.Texture7,
                TexturingKeys.Texture8,
                TexturingKeys.Texture9
            };

        /// <summary>
        /// Available diffuse texture keys.
        /// </summary>
        private static readonly List<ParameterKey<Graphics.Texture>> DiffuseTextureKeys = new List<ParameterKey<Graphics.Texture>>()
            {
                MaterialTexturingKeys.DiffuseTexture0,
                MaterialTexturingKeys.DiffuseTexture1,
                MaterialTexturingKeys.DiffuseTexture2,
                MaterialTexturingKeys.DiffuseTexture3
            };

        /// <summary>
        /// Available specular texture keys.
        /// </summary>
        private static readonly List<ParameterKey<Graphics.Texture>> SpecularTextureKeys = new List<ParameterKey<Graphics.Texture>>()
            {
                MaterialTexturingKeys.SpecularTexture0,
                MaterialTexturingKeys.SpecularTexture1,
                MaterialTexturingKeys.SpecularTexture2
            };

        /// <summary>
        /// Available normal map texture keys.
        /// </summary>
        private static readonly List<ParameterKey<Graphics.Texture>> NormalMapTextureKeys = new List<ParameterKey<Graphics.Texture>>()
            {
                MaterialTexturingKeys.NormalMapTexture0,
                MaterialTexturingKeys.NormalMapTexture1
            };

        /// <summary>
        /// Available displacement texture keys.
        /// </summary>
        private static readonly List<ParameterKey<Graphics.Texture>> DisplacementTextureKeys = new List<ParameterKey<Graphics.Texture>>()
            {
                MaterialTexturingKeys.DisplacementTexture0
            };

        #endregion

        #region Private members

        /// <summary>
        /// Generic method to get a texture key
        /// </summary>
        private delegate ParameterKey<Graphics.Texture> TextureKeyGetter(MaterialTextureVisitor textureVisitor);

        /// <summary>
        /// Index of the next candidate in the default texture key pool.
        /// </summary>
        private int nextDefaultIndex = 0;

        /// <summary>
        /// Index of the next candidate in the diffuse texture key pool.
        /// </summary>
        private int nextDiffuseIndex = 0;

        /// <summary>
        /// Index of the next candidate in the specular texture key pool.
        /// </summary>
        private int nextSpecularIndex = 0;

        /// <summary>
        /// Index of the next candidate in the normal map texture key pool.
        /// </summary>
        private int nextNormalMapIndex = 0;

        /// <summary>
        /// Index of the next candidate in the displacement texture key pool.
        /// </summary>
        private int nextDisplacementIndex = 0;

        /// <summary>
        /// List of already used texture keys.
        /// </summary>
        private List<ParameterKey<Graphics.Texture>> usedTextureKeys = new List<ParameterKey<Graphics.Texture>>();

        #endregion

        #region Public methods

        public MaterialTextureVisitor(MaterialDescription mat) : base(mat)
        {
        }

        /// <summary>
        /// Reset the indices.
        /// </summary>
        public void ResetTextureIndices()
        {
            nextDefaultIndex = 0;
            nextDiffuseIndex = 0;
            nextSpecularIndex = 0;
            nextNormalMapIndex = 0;
            nextDisplacementIndex = 0;
        }

        /// <summary>
        /// Assign the default texture keys to the texture slots.
        /// </summary>
        /// <param name="nodes">List of nodes.</param>
        public void AssignDefaultTextureKeys(IEnumerable<MaterialTextureNode> nodes, IEnumerable<NodeParameterSampler> samplers)
        {
            AssignParameterKeys(nodes, samplers, GetNextDefaultTextureKey);
        }

        /// <summary>
        /// Assign the default texture keys to the texture slots.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        public void AssignDefaultTextureKeys(IMaterialNode node)
        {
            AssignParameterKeys(node, GetNextDefaultTextureKey);
        }

        /// <summary>
        /// Assign the diffuse texture keys to the texture slots.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        public void AssignDiffuseTextureKeys(IMaterialNode node)
        {
            AssignParameterKeys(node, GetNextDiffuseTextureKey);
        }

        /// <summary>
        /// Assign the specular texture keys to the texture slots.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        public void AssignSpecularTextureKeys(IMaterialNode node)
        {
            AssignParameterKeys(node, GetNextSpecularTextureKey);
        }

        /// <summary>
        /// Assign the normal map texture keys to the texture slots.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        public void AssignNormalMapTextureKeys(IMaterialNode node)
        {
            AssignParameterKeys(node, GetNextNormalMapTextureKey);
        }

        /// <summary>
        /// Assign the displacement texture keys to the texture slots.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        public void AssignDisplacementTextureKeys(IMaterialNode node)
        {
            AssignParameterKeys(node, GetNextDisplacementTextureKey);
        }

        /// <summary>
        /// Test if the tree can be reduced to a single texture or color.
        /// </summary>
        /// <param name="node">The node to explore.</param>
        /// <param name="texcoord">The unique texcoord if applicable.</param>
        /// <returns>A boolean stating this test.</returns>
        public bool HasUniqueTexcoord(IMaterialNode node, out TextureCoordinate texcoord)
        {
            var allTextures = GatherTextureValues(node).Distinct().ToList();
            texcoord = TextureCoordinate.TexcoordNone;

            foreach (var texSlot in allTextures)
            {
                if (texcoord == TextureCoordinate.TexcoordNone)
                    texcoord = texSlot.TexcoordIndex;

                if (texcoord != texSlot.TexcoordIndex)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get all the textures needed for this node.
        /// </summary>
        /// <param name="node">The node to explore.</param>
        /// <returns>The list containing all the textures.</returns>
        public List<MaterialTextureNode> GetAllTextureValues(IMaterialNode node)
        {
            return GatherTextureValues(node).Distinct().ToList();
        }

        /// <summary>
        /// Get all the textures needed for this node with the potential generic parameters.
        /// </summary>
        /// <param name="node">The node to explore.</param>
        /// <returns>The list containing all the textures.</returns>
        public List<MaterialTextureNode> GetAllTextureValuesWithGenerics(IMaterialNode node)
        {
            return GatherTextureValuesWithGenerics(node).Distinct().ToList();
        }

        /// <summary>
        /// Get all the textures needed for this node.
        /// </summary>
        /// <param name="node">The node to explore.</param>
        /// <returns>The list containing all the textures.</returns>
        public List<NodeParameterSampler> GetAllSamplerValues(IMaterialNode node)
        {
            return GatherSamplerValues(node).Distinct().ToList();
        }

        /// <summary>
        /// Get all the textures for this model.
        /// </summary>
        /// <returns>The list containing all the textures.</returns>
        public List<MaterialTextureNode> GetAllModelTextureValues()
        {
            var returnList = new List<MaterialTextureNode>();
            foreach (var reference in Material.ColorNodes)
            {
                returnList.AddRange(GatherTextureValues(Material.FindNode(reference.Value)));
            }
            return returnList.Distinct().ToList();
        }

        /// <summary>
        /// Get all the textures for this model.
        /// </summary>
        /// <returns>The list containing all the textures even the ones in the generics.</returns>
        public List<MaterialTextureNode> GetAllModelTextureValuesWithGenerics()
        {
            var returnList = new List<MaterialTextureNode>();
            foreach (var reference in Material.ColorNodes)
            {
                returnList.AddRange(GatherTextureValuesWithGenerics(Material.FindNode(reference.Value)));
            }
            return returnList.Distinct().ToList();
        }

        /// <summary>
        /// Get all the Sampler from the generics for this model.
        /// </summary>
        /// <returns>The list containing all the textures.</returns>
        public List<NodeParameterSampler> GetAllSamplerValues()
        {
            var returnList = new List<NodeParameterSampler>();
            foreach (var reference in Material.ColorNodes)
            {
                returnList.AddRange(GatherSamplerValues(Material.FindNode(reference.Value)));
            }
            return returnList.Distinct().ToList();
        }

        /// <summary>
        /// Get all the texture from this material.
        /// </summary>
        /// <returns>The list containing all the textures</returns>
        public List<MaterialTextureNode> GetAllTextureValues()
        {
            var returnList = new List<MaterialTextureNode>();
            foreach (var tree in Material.Nodes)
                returnList.AddRange(GatherTextureValues(tree.Value));
            return returnList.Distinct().ToList();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Assign the Keys to the textures in the tree.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <param name="getTextureKey">The delegate function to get the correct key.</param>
        private void AssignParameterKeys(IMaterialNode node, TextureKeyGetter getTextureKey)
        {
            var allTextures = GatherTextureValues(node).Distinct().ToList();
            var textureKeys = new Dictionary<string, ParameterKey<Graphics.Texture>>();

            foreach (var texSlot in allTextures)
            {
                //TODO: compare texture sampling method
                ParameterKey<Graphics.Texture> pk;
                var textureName = texSlot.TextureName;
                if (textureName == null || !textureKeys.TryGetValue(textureName, out pk))
                {
                    pk = getTextureKey(this);
                    if (textureName != null)
                        textureKeys.Add(textureName, pk);
                }

                texSlot.UsedParameterKey = pk;
                texSlot.Sampler.SamplerParameterKey = TexturingKeys.Sampler;
            }
        }

        /// <summary>
        /// Assign the Keys to the textures and the samplers in the list.
        /// </summary>
        /// <param name="nodes">List of nodes.</param>
        /// <param name="samplers">The list of sampler existing outside of MaterialTextureNode</param>
        /// <param name="getTextureKey">The delegate function to get the correct key.</param>
        private void AssignParameterKeys(IEnumerable<MaterialTextureNode> nodes, IEnumerable<NodeParameterSampler> samplers, TextureKeyGetter getTextureKey)
        {
            var textureKeys = new Dictionary<IMaterialNode, ParameterKey<Graphics.Texture>>();
            var samplerKeys = new Dictionary<SamplerDescription, ParameterKey<SamplerState>>();
            int samplerIndex = 0;

            // assign the predefined keys
            foreach (var texSlot in nodes.Distinct())
            {
                if (!texSlot.AutoAssignKey && texSlot.Key != null)
                {
                    texSlot.UsedParameterKey = (ParameterKey<Graphics.Texture>)texSlot.Key;
                    textureKeys.Add(texSlot, texSlot.UsedParameterKey);
                    usedTextureKeys.Add(texSlot.UsedParameterKey);
                }
            }

            // assign/generate all the keys
            foreach (var texSlot in nodes.Distinct())
            {
                ParameterKey<Graphics.Texture> textureParameterKey;
                if (!textureKeys.TryGetValue(texSlot, out textureParameterKey))
                {
                    textureParameterKey = getTextureKey(this);
                    textureKeys.Add(texSlot, textureParameterKey);
                }
                texSlot.UsedParameterKey = textureParameterKey;

                SetSamplerKey(texSlot.Sampler, samplerKeys, ref samplerIndex);
            }

            if (samplers != null)
            {
                foreach (var gen in samplers)
                    SetSamplerKey(gen, samplerKeys, ref samplerIndex);
            }
        }

        /// <summary>
        /// Get the sampler key, a new one or an existing one.
        /// </summary>
        /// <param name="sampler">The sampler.</param>
        /// <param name="samplerKeys">The already defined keys.</param>
        /// <param name="samplerIndex">The current index of the sampler.</param>
        /// <returns>The ParameterKey.</returns>
        private void SetSamplerKey(NodeParameterSampler sampler, Dictionary<SamplerDescription, ParameterKey<SamplerState>> samplerKeys, ref int samplerIndex)
        {
            var state = new SamplerDescription { Filtering = sampler.Filtering, AddressModeU = sampler.AddressModeU, AddressModeV = sampler.AddressModeV, AddressModeW = TextureAddressMode.Wrap };
            ParameterKey<SamplerState> samplerParameterKey;
            if (!samplerKeys.TryGetValue(state, out samplerParameterKey))
            {
                samplerParameterKey = GetDefaultSamplerKey(samplerIndex);
                ++samplerIndex;
                samplerKeys.Add(state, samplerParameterKey);
            }
            sampler.SamplerParameterKey = samplerParameterKey;
        }

        /// <summary>
        /// Generic function to get the next texture key.
        /// </summary>
        /// <param name="textureKeysList">The list of available texture keys.</param>
        /// <param name="nextIndex">the index of the next parameter key.</param>
        /// <returns>The parameter key.</returns>
        private ParameterKey<Graphics.Texture> GetNextTextureKey(List<ParameterKey<Graphics.Texture>> textureKeysList, ref int nextIndex)
        {
            while (usedTextureKeys.Contains(textureKeysList[nextIndex]) && nextIndex < textureKeysList.Count)
            {
                ++nextIndex;
            }

            if (nextIndex == textureKeysList.Count)
                throw new IndexOutOfRangeException("There is no more available texture key.");

            return textureKeysList[nextIndex++];
        }

        /// <summary>
        /// Gather all the textures in the node hierarchy.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <returns>A collection of MaterialTextureNode.</returns>
        private IEnumerable<MaterialTextureNode> GatherTextureValues(IMaterialNode node)
        {
            var materialContext = new MaterialContext { Material = Material, ExploreGenerics = false };
            return GatherTextures(node, materialContext);
        }

        /// <summary>
        /// Gather all the textures in the node hierarchy, even the ones behind the generics.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <returns>A collection of MaterialTextureNode.</returns>
        private IEnumerable<MaterialTextureNode> GatherTextureValuesWithGenerics(IMaterialNode node)
        {
            var materialContext = new MaterialContext { Material = Material, ExploreGenerics = true };
            return GatherTextures(node, materialContext);
        }

        /// <summary>
        /// Common gather texture function.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <param name="materialContext">The visitor context.</param>
        /// <returns>A collection of MaterialTextureNode.</returns>
        private IEnumerable<MaterialTextureNode> GatherTextures(IMaterialNode node, MaterialContext materialContext)
        {
            var textureValues = new List<MaterialTextureNode>();
            node.VisitNodes((context, nodeEntry) =>
            {
                var textureValue = nodeEntry.Node as MaterialTextureNode;
                if (textureValue != null)
                {
                    textureValues.Add(textureValue);
                }
            }, materialContext);
            return textureValues;
        }

        /// <summary>
        /// Gather all the sampler generics in the node hierarchy.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <returns>A collection of NodeParameterSampler.</returns>
        private IEnumerable<NodeParameterSampler> GatherSamplerValues(IMaterialNode node)
        {
            var samplerValues = new List<NodeParameterSampler>();
            node.VisitNodes((context, nodeEntry) =>
            {
                var shaderClassNode = nodeEntry.Node as MaterialShaderClassNode;
                if (shaderClassNode != null)
                {
                    foreach (var gen in shaderClassNode.Generics)
                    {
                        var genSampler = gen.Value as NodeParameterSampler;
                        if (genSampler != null)
                        {
                            samplerValues.Add(genSampler);
                        }
                    }
                }
            }, new MaterialContext { Material = Material, ExploreGenerics = false });
            return samplerValues;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Get the next texture parameter key from the default pool.
        /// </summary>
        /// <returns>A parameter key.</returns>
        private static ParameterKey<Graphics.Texture> GetNextDefaultTextureKey(MaterialTextureVisitor textureVisitor)
        {
            return textureVisitor.GetNextTextureKey(DefaultTextureKeys, ref textureVisitor.nextDefaultIndex);
        }

        /// <summary>
        /// Get the next texture parameter key from the diffuse pool.
        /// </summary>
        /// <returns>A parameter key.</returns>
        private static ParameterKey<Graphics.Texture> GetNextDiffuseTextureKey(MaterialTextureVisitor textureVisitor)
        {
            return textureVisitor.GetNextTextureKey(DiffuseTextureKeys, ref textureVisitor.nextDiffuseIndex);
        }

        /// <summary>
        /// Get the next texture parameter key from the specular pool.
        /// </summary>
        /// <returns>A parameter key.</returns>
        private static ParameterKey<Graphics.Texture> GetNextSpecularTextureKey(MaterialTextureVisitor textureVisitor)
        {
            return textureVisitor.GetNextTextureKey(SpecularTextureKeys, ref textureVisitor.nextSpecularIndex);
        }

        /// <summary>
        /// Get the next texture parameter key from the normal map pool.
        /// </summary>
        /// <returns>A parameter key.</returns>
        private static ParameterKey<Graphics.Texture> GetNextNormalMapTextureKey(MaterialTextureVisitor textureVisitor)
        {
            return textureVisitor.GetNextTextureKey(NormalMapTextureKeys, ref textureVisitor.nextNormalMapIndex);
        }

        /// <summary>
        /// Get the next texture parameter key from the displacement pool.
        /// </summary>
        /// <returns>A parameter key.</returns>
        private static ParameterKey<Graphics.Texture> GetNextDisplacementTextureKey(MaterialTextureVisitor textureVisitor)
        {
            return textureVisitor.GetNextTextureKey(DisplacementTextureKeys, ref textureVisitor.nextDisplacementIndex);
        }

        /// <summary>
        /// Get the ParameterKey of generic sampler.
        /// </summary>
        /// <param name="i">The id of the texture.</param>
        /// <returns>The corresponding ParameterKey.</returns>
        private static ParameterKey<SamplerState> GetDefaultSamplerKey(int i)
        {
            switch (i)
            {
                case 0:
                    return TexturingKeys.Sampler0;
                case 1:
                    return TexturingKeys.Sampler1;
                case 2:
                    return TexturingKeys.Sampler2;
                case 3:
                    return TexturingKeys.Sampler3;
                case 4:
                    return TexturingKeys.Sampler4;
                case 5:
                    return TexturingKeys.Sampler5;
                case 6:
                    return TexturingKeys.Sampler6;
                case 7:
                    return TexturingKeys.Sampler7;
                case 8:
                    return TexturingKeys.Sampler8;
                case 9:
                    return TexturingKeys.Sampler9;
                default:
                    throw new ArgumentOutOfRangeException("Asked for " + i + " but no more than 10 default textures are currently supported");
            }
        }

        #endregion

        private struct SamplerDescription
        {
            public TextureFilter Filtering;
            public TextureAddressMode AddressModeU;
            public TextureAddressMode AddressModeV;
            public TextureAddressMode AddressModeW;
        }
    }
}
