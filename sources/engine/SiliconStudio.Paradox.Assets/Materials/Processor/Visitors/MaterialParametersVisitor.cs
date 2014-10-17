// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects.Data;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialParametersVisitor : MaterialBaseVisitor
    {
        public MaterialParametersVisitor(MaterialDescription mat)
            : base(mat)
        {
        }

        public ParameterCollectionData GetParameters()
        {
            var parameters = new ParameterCollectionData();
            foreach (var startNodeName in Material.ColorNodes)
            {
                var startNode = Material.FindNode(startNodeName.Value);
                if (startNode != null)
                {
                    GetParametersFromNode(startNode, parameters);
                }
            }
            return parameters;
        }

        /// <summary>
        /// Gather all the parameters in the node hierarchy.
        /// </summary>
        /// <param name="node">The node to look into.</param>
        /// <param name="parameters">The parameter collection to fill.</param>
        private void GetParametersFromNode(IMaterialNode node, ParameterCollectionData parameters)
        {
            if (node == null)
                return;
            
            node.VisitNodes((context, nodeEntry) =>
            {
                var shaderNode = nodeEntry.Node as MaterialShaderClassNode;
                if (shaderNode != null)
                {
                    //foreach (var member in shaderNode.Members)
                    foreach (var member in shaderNode.GetParameters(context))
                        parameters.Set(member.Key, member.Value);
                }
            }, new MaterialContext { Material = Material, ExploreGenerics = false });
        }
    }
}
