// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialTreeReducer : MaterialBaseVisitor
    {
        #region Private members

        /// <summary>
        /// Reduction status of each tree.
        /// </summary>
        private readonly Dictionary<string, ReductionStatus> treeReductionStatus;

        #endregion

        #region Publice members
        
        /// <summary>
        /// Reduced form of each tree.
        /// </summary>
        public readonly Dictionary<string, IMaterialNode> ReducedTrees;
        
        #endregion

        #region Public methods

        public MaterialTreeReducer(MaterialDescription mat) : base(mat)
        {
            treeReductionStatus = Material.Nodes.ToDictionary(x => x.Key, x => ReductionStatus.None);
            ReducedTrees = new Dictionary<string, IMaterialNode>();
        }

        /// <summary>
        /// Reduce all the trees.
        /// </summary>
        public void ReduceTrees()
        {
            foreach (var tree in Material.Nodes)
            {
                BeginReduction(tree.Key);
            }
        }

        /// <summary>
        /// Get the list of all the reducible sub-trees for this tree. If a tree is entirely reducible, only the rootnode will appear
        /// </summary>
        /// <returns>The list of all the reducible sub-trees</returns>
        public List<IMaterialNode> GetReducibleSubTrees(IMaterialNode node)
        {
            var resultList = new List<IMaterialNode>();
            var criterion = new ReductionCriteria { TexcoordIndex = TextureCoordinate.TexcoordNone };
            if (BuildReducibleSubTreesList(node, resultList, out criterion))
                AddNodeToReduceList(node, resultList);
            return resultList;
        }

        /// <summary>
        /// Checks if the node can be reduced and adds to the list the ones that cannot be reduced further.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <param name="resultList">The list of node to reduce.</param>
        /// <param name="criterion">The current criteria to reduce the node.</param>
        /// <returns>True if the node can be reduced, false otherwise.</returns>
        private bool BuildReducibleSubTreesList(IMaterialNode node, List<IMaterialNode> resultList, out ReductionCriteria criterion)
        {
            if (node is IMaterialValueNode)
            {
                criterion.TexcoordIndex = node is MaterialTextureNode ? ((MaterialTextureNode)node).TexcoordIndex : TextureCoordinate.TexcoordNone;
                criterion.AddressModeU = node is MaterialTextureNode ? ((MaterialTextureNode)node).Sampler.AddressModeU : (TextureAddressMode?)null;
                criterion.AddressModeV = node is MaterialTextureNode ? ((MaterialTextureNode)node).Sampler.AddressModeV : (TextureAddressMode?)null;

                var isSingleReducible = node.IsReducible;
                if (node is MaterialFloatNode)
                    isSingleReducible &= (((MaterialFloatNode)node).Key == null);
                if (node is MaterialFloat4Node)
                    isSingleReducible &= (((MaterialFloat4Node)node).Key == null);
                if (node is MaterialColorNode)
                    isSingleReducible &= (((MaterialColorNode)node).Key == null);

                return isSingleReducible;
            }

            var refNode = node as MaterialReferenceNode;
            if (refNode != null)
            {
                return BuildReducibleSubTreesList(Material.FindNode(refNode.Name), resultList, out criterion);
            }

            criterion.TexcoordIndex = TextureCoordinate.TexcoordNone;
            criterion.AddressModeU = (TextureAddressMode?)null;
            criterion.AddressModeV = (TextureAddressMode?)null;
            
            bool? checkIsReducible = null;
            ReductionCriteria? checkCriterion = null;
            var reducibleNodes = new List<IMaterialNode>();

            foreach (var subNodeIt in node.GetChildren(Material))
            {
                var subNode = subNodeIt.Node;
                ReductionCriteria childCriterion;
                var childIsReducible = BuildReducibleSubTreesList(subNode, resultList, out childCriterion);
                if (!checkIsReducible.HasValue)
                {
                    checkIsReducible = childIsReducible;
                    checkCriterion = childCriterion;
                }
                else
                {
                    checkIsReducible = checkIsReducible.Value && childIsReducible && CompatibleCriteria(childCriterion, checkCriterion.Value);
                    checkCriterion = MergeCriteria(checkCriterion.Value, childCriterion);
                }

                if (childIsReducible)
                    reducibleNodes.Add(subNode);
            }

            if (checkCriterion.HasValue)
            {
                criterion = checkCriterion.Value;
            }

            if (checkIsReducible.HasValue && !checkIsReducible.Value)
            {
                // if one child is not reducible, add all the reducible ones to the list
                foreach (var reducibleNode in reducibleNodes)
                    AddNodeToReduceList(reducibleNode, resultList);

                return false;
            }
            
            return node.IsReducible;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Starts the reduction of this reference.
        /// </summary>
        /// <param name="referenceName">The name of the reference.</param>
        private void BeginReduction(string referenceName)
        {
            if (referenceName == null)
                return;

            if (!treeReductionStatus.ContainsKey(referenceName))
                return;

            var status = treeReductionStatus[referenceName];

            if (status == ReductionStatus.None)
            {
                var node = Material.Nodes[referenceName];
                treeReductionStatus[referenceName] = ReductionStatus.InProgress;
                var computedNode = Reduce(node);
                if (computedNode != null)
                    ReducedTrees[referenceName] = computedNode;
                treeReductionStatus[referenceName] = ReductionStatus.Completed;
            }
            else if (status == ReductionStatus.InProgress)
            {
                treeReductionStatus[referenceName] = ReductionStatus.Completed;
                // TODO: cycle - log error
            }
        }

        /// <summary>
        /// Reduce the binaryNode to its most compact form.
        /// </summary>
        /// <param name="node">The IMaterialNode binaryNode to reduce.</param>
        /// <returns>The reduced binaryNode.</returns>
        private IMaterialNode Reduce(IMaterialNode node)
        {
            if (node is MaterialBinaryNode)
                return Reduce(node as MaterialBinaryNode);
            if (node is MaterialReferenceNode)
            {
                var referenceName = (node as MaterialReferenceNode).Name;
                if (referenceName != null)
                {
                    BeginReduction(referenceName);

                    if (ReducedTrees.ContainsKey(referenceName))
                        return ReducedTrees[referenceName];

                    return null;
                }
            }
            return node;
        }

        /// <summary>
        /// Reduce the binaryNode to its most compact form.
        /// </summary>
        /// <param name="binaryNode">The MaterialBinaryNode binaryNode to reduce.</param>
        /// <returns>The reduced binaryNode.</returns>
        private IMaterialNode Reduce(MaterialBinaryNode binaryNode)
        {
            var left = Reduce(binaryNode.LeftChild);
            var right = Reduce(binaryNode.RightChild);

            bool canReduce = true;
            //if (binaryNode.LeftChild is IMaterialValueNode)
            //    canReduce &= !(binaryNode.LeftChild as IMaterialValueNode).IsParameter;
            //if (binaryNode.RightChild is IMaterialValueNode)
            //    canReduce &= !(binaryNode.RightChild as IMaterialValueNode).IsParameter;

            if (canReduce)
            {
                if (left is MaterialFloatNode)
                {
                    if (right is MaterialFloatNode)
                    {
                        (left as MaterialFloatNode).Value = MaterialReductionUtils.MixFloat((left as MaterialFloatNode).Value, (right as MaterialFloatNode).Value, binaryNode.Operand);
                        return left;
                    }
                    if (right is MaterialFloat4Node)
                    {
                        var value = (left as MaterialFloatNode).Value;
                        (right as MaterialFloat4Node).Value = MaterialReductionUtils.MixFloat4(new Vector4(value, value, value, value), (right as MaterialFloat4Node).Value, binaryNode.Operand);
                        return right;
                    }
                    if (right is MaterialColorNode)
                    {
                        var value = (left as MaterialFloatNode).Value;
                        (right as MaterialColorNode).Value = MaterialReductionUtils.MixColor(new Color4(value, value, value, value), (right as MaterialColorNode).Value, binaryNode.Operand);
                        return right;
                    }
                }
                else if (left is MaterialFloat4Node)
                {
                    if (right is MaterialFloatNode)
                    {
                        var value = (right as MaterialFloatNode).Value;
                        (left as MaterialFloat4Node).Value = MaterialReductionUtils.MixFloat4((left as MaterialFloat4Node).Value, new Vector4(value, value, value, value), binaryNode.Operand);
                        return left;
                    }
                    if (right is MaterialFloat4Node)
                    {
                        (left as MaterialFloat4Node).Value = MaterialReductionUtils.MixFloat4((left as MaterialFloat4Node).Value, (right as MaterialFloat4Node).Value, binaryNode.Operand);
                        return left;
                    }
                    if (right is MaterialColorNode)
                    {
                        var value = (left as MaterialFloat4Node).Value;
                        (left as MaterialFloat4Node).Value = MaterialReductionUtils.MixFloat4(value, (right as MaterialColorNode).Value.ToVector4(), binaryNode.Operand);
                        return left;
                    }
                }
                else if (left is MaterialColorNode)
                {
                    if (right is MaterialFloatNode)
                    {
                        var value = (right as MaterialFloatNode).Value;
                        (left as MaterialColorNode).Value = MaterialReductionUtils.MixColor((left as MaterialColorNode).Value, new Color4(value, value, value, value), binaryNode.Operand);
                        return left;
                    }
                    if (right is MaterialFloat4Node)
                    {
                        (right as MaterialFloat4Node).Value = MaterialReductionUtils.MixFloat4((left as MaterialColorNode).Value.ToVector4(), (right as MaterialFloat4Node).Value, binaryNode.Operand);
                        return right;
                    }
                    if (right is MaterialColorNode)
                    {
                        var value = (left as MaterialColorNode).Value;
                        (left as MaterialColorNode).Value = MaterialReductionUtils.MixColor((left as MaterialColorNode).Value, (right as MaterialColorNode).Value, binaryNode.Operand);
                        return left;
                    }
                }
            }

            binaryNode.LeftChild = left;
            binaryNode.RightChild = right;
            return binaryNode;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Add a node to the list of of nodes to reduce.
        /// </summary>
        /// <param name="node">The node to add.</param>
        /// <param name="reduceList">The list of node to reduce.</param>
        private static void AddNodeToReduceList(IMaterialNode node, List<IMaterialNode> reduceList)
        {
            if (!(node is IMaterialValueNode || node is MaterialTextureNode || node is MaterialShaderClassNode))
                reduceList.Add(node);
        }

        /// <summary>
        /// Check if the two criteria are compatible.
        /// </summary>
        /// <param name="baseCriteria">The base criteria.</param>
        /// <param name="newCriteria">The new criteria.</param>
        /// <returns>True if they are compatible.</returns>
        private static bool CompatibleCriteria(ReductionCriteria baseCriteria, ReductionCriteria newCriteria)
        {
            var result = baseCriteria.TexcoordIndex == TextureCoordinate.TexcoordNone || newCriteria.TexcoordIndex == TextureCoordinate.TexcoordNone || baseCriteria.TexcoordIndex == newCriteria.TexcoordIndex;
            result = result && (!baseCriteria.AddressModeU.HasValue || !newCriteria.AddressModeU.HasValue || baseCriteria.AddressModeU.Value == newCriteria.AddressModeU.Value);
            result = result && (!baseCriteria.AddressModeV.HasValue || !newCriteria.AddressModeV.HasValue || baseCriteria.AddressModeV.Value == newCriteria.AddressModeV.Value);
            return result;
        }

        /// <summary>
        /// Merge the info of the two compatible criteria.
        /// </summary>
        /// <param name="baseCriteria">The base criteria.</param>
        /// <param name="newCriteria">The new criteria.</param>
        /// <returns>The merged criteria.</returns>
        private static ReductionCriteria MergeCriteria(ReductionCriteria baseCriteria, ReductionCriteria newCriteria)
        {
            var result = new ReductionCriteria
                {
                    TexcoordIndex = baseCriteria.TexcoordIndex == TextureCoordinate.TexcoordNone ? newCriteria.TexcoordIndex : baseCriteria.TexcoordIndex,
                    AddressModeU = baseCriteria.AddressModeU.HasValue ? baseCriteria.AddressModeU : (newCriteria.AddressModeU.HasValue ? newCriteria.AddressModeU.Value : (TextureAddressMode?)null),
                    AddressModeV = baseCriteria.AddressModeV.HasValue ? baseCriteria.AddressModeV : (newCriteria.AddressModeV.HasValue ? newCriteria.AddressModeV.Value : (TextureAddressMode?)null)
                };
            return result;
        }

        #endregion

        private enum ReductionStatus
        {
            None,
            InProgress,
            Completed
        }

        private struct ReductionCriteria
        {
            public TextureCoordinate TexcoordIndex;
            public TextureAddressMode? AddressModeU;
            public TextureAddressMode? AddressModeV;
        }
    }
}
