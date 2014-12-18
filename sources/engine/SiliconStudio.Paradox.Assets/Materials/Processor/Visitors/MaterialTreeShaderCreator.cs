// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialTreeShaderCreator : MaterialBaseVisitor
    {
        /// <summary>
        /// The shader build statuses.
        /// </summary>
        private Dictionary<string, ShaderBuildStatus> shaderBuildStatuses;

        /// <summary>
        /// Flag to change some shaders.
        /// </summary>
        private bool shaderForReduction = false;

        /// <summary>
        /// A flag stating if this is for displacement purpose.
        /// </summary>
        private bool displacementShader = false;

        /// <summary>
        /// The constant values.
        /// </summary>
        private ParameterCollection constantValues;

        /// <summary>
        /// All the shaders.
        /// </summary>
        public Dictionary<string, ShaderSource> ModelShaderSources;

        /// <summary>
        /// The error logger.
        /// </summary>
        public readonly LoggerResult Logger;
        
        public MaterialTreeShaderCreator(MaterialDescription mat) : base(mat)
        {
            shaderBuildStatuses = mat.Nodes.ToDictionary(x => x.Key, x => ShaderBuildStatus.None);
            ModelShaderSources = new Dictionary<string, ShaderSource>();
            Logger = new LoggerResult();
        }
        
        /// <summary>
        /// Generate one shader.
        /// </summary>
        public ShaderMixinSource GenerateShaderForReduction(IMaterialNode materialNode)
        {
            shaderForReduction = true;

            var textureVisitor = new MaterialTextureVisitor(Material);
            var allTextures  = textureVisitor.GetAllTextureValues(materialNode);
            textureVisitor.AssignDefaultTextureKeys(allTextures.Distinct(), null);
            return MaterialUtility.GetShaderMixinSource(GetShaderSource(materialNode));
        }

        /// <summary>
        /// Generate all the shaders for this model, assign keys for texture and samplers.
        /// </summary>
        /// <returns>A dictionary of the shaders.</returns>
        public ParameterCollection GenerateModelShaders()
        {
            AssignModelTextureKeys();
            ModelShaderSources = new Dictionary<string, ShaderSource>();
            constantValues = new ParameterCollection();
            shaderBuildStatuses = Material.Nodes.ToDictionary(x => x.Key, x => ShaderBuildStatus.None);
            var result = new ParameterCollection();
            shaderForReduction = false;
            foreach (var reference in Material.ColorNodes)
            {
                if (reference.Key != null)
                {
                    if (reference.Value != null)
                    {
                        BeginShaderCreation(reference.Value, reference.Key == MaterialParameters.DisplacementMap);

                        ShaderSource shaderSource;
                        if (ModelShaderSources.TryGetValue(reference.Value, out shaderSource))
                        {
                            var sms = MaterialUtility.GetShaderMixinSource(shaderSource);
                            if (sms != null)
                            {
                                result.Add(reference.Key, sms);
                                continue;
                            }
                        }
                    }
                    Logger.Error("[Material] Shader creation failed. The key " + reference.Key.Name + " did not produce any shader.");
                }
                else
                {
                    Logger.Error("[Material] Shader creation failed. The key " + reference.Key.Name + " in ColorNodes is not a ShaderMixinSource parameter key.");
                }
            }
            constantValues.CopyTo(result);
            return result;
        }
        
        /// <summary>
        /// Assign the default texture keys to this model.
        /// </summary>
        private void AssignModelTextureKeys()
        {
            var textureVisitor = new MaterialTextureVisitor(Material);
            var allTextures = new List<MaterialTextureNode>();
            var allSampler = new List<NodeParameterSampler>();
            foreach (var referenceName in Material.ColorNodes.Select(x => x.Value))
            {
                var startNode = Material.FindNode(referenceName);
                if (startNode != null)
                {
                    allTextures.AddRange(textureVisitor.GetAllTextureValuesWithGenerics(startNode));
                    allSampler.AddRange(textureVisitor.GetAllSamplerValues(startNode));
                }
            }
            textureVisitor.AssignDefaultTextureKeys(allTextures.Distinct(), allSampler.Distinct());
        }

        /// <summary>
        /// Creates shaders with multiple keys.
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        /// <param name="useForDisplacement">A flag stating that this shader will be used for displacement.</param>
        private void BeginShaderCreation(string referenceName, bool useForDisplacement)
        {
            if (referenceName == null || !shaderBuildStatuses.ContainsKey(referenceName))
                return;

            displacementShader = useForDisplacement;

            var status = shaderBuildStatuses[referenceName];

            if (status == ShaderBuildStatus.None)
            {
                var node = Material.FindNode(referenceName);
                if (node == null)
                {
                    Logger.Error("[Material] There is no node with the name " + referenceName + ".");
                    shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
                    return;
                }

                shaderBuildStatuses[referenceName] = ShaderBuildStatus.InProgress;
                ModelShaderSources[referenceName] = GetShaderSource(node);
                shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
            }
            else if (status == ShaderBuildStatus.InProgress)
            {
                shaderBuildStatuses[referenceName] = ShaderBuildStatus.Completed;
                Logger.Error("[Material] The node reference " + referenceName + " is part of a cycle.");
            }
        }

        /// <summary>
        /// Gets the shader source.
        /// </summary>
        /// <param name="node">The node to process.</param>
        /// <returns>The shader source.</returns>
        private ShaderSource GetShaderSource(IMaterialNode node)
        {
            if (node == null)
                return new ShaderClassSource("ComputeColor");

            return node.GenerateShaderSource(null);
        }

        private enum ShaderBuildStatus
        {
            None,
            InProgress,
            Completed
        }
    }
}
