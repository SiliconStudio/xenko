// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A class describing the path of a node, relative to a root node. The path can cross references, array, etc.
    /// </summary>
    public class ModelNodePath
    {
        private class NodePathItemIndex
        {
            public object Value;
            public override string ToString() { return string.Format("[{0}]", Value); }
        }

        private class NodePathItemMember
        {
            public string Name;
            public override string ToString() { return string.Format(".{0}", Name); } }

        private class NodePathItemTarget
        {
            public override string ToString() { return "-> (Target)"; }
        }

        private readonly List<object> path = new List<object>();
        private bool targetIsRootNode;

        private ModelNodePath()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNodePath"/> with the given root node.
        /// </summary>
        /// <param name="rootNode">The root node to represent with this instance of <see cref="ModelNodePath"/>.</param>
        /// <remarks>This constructor should be used for path to a root node only. To create a path to a child node, use <see cref="GetChildPath"/>.</remarks>
        public ModelNodePath(IModelNode rootNode)
        {
            RootNode = rootNode;
            targetIsRootNode = true;
        }

        /// <summary>
        /// Gets whether this path is a valid path.
        /// </summary>
        public bool IsValid { get { return path.Count > 0 || targetIsRootNode; } }

        /// <summary>
        /// Gets the root node of this path.
        /// </summary>
        public IModelNode RootNode { get; private set; }

        /// <summary>
        /// Gets the node corresponding to this path.
        /// </summary>
        /// <returns>The node corresponding to this path.</returns>
        /// <exception cref="InvalidOperationException">The path is invalid.</exception>
        public IModelNode GetNode()
        {
            if (!IsValid)
                throw new InvalidOperationException("The node path is invalid.");

            IModelNode node = RootNode;
            foreach (var itemPath in path)
            {
                var member = itemPath as NodePathItemMember;
                var target = itemPath as NodePathItemTarget;
                var index = itemPath as NodePathItemIndex;
                if (member != null)
                {
                    node = node.Children.Single(x => x.Name == member.Name);
                }
                else if (target != null)
                {
                    var objectRefererence = (ObjectReference)node.Content.Reference;
                    node = objectRefererence.TargetNode;
                }
                else if (index != null)
                {
                    var enumerableReference = (ReferenceEnumerable)node.Content.Reference;
                    var objectRefererence = enumerableReference.Single(x => Equals(x.Index, index.Value));
                    node = objectRefererence.TargetNode;
                }
            }
            return node;
        }
        
        /// <summary>
        /// Computes a <see cref="ModelNodePath"/> corresponding to the given <see cref="target"/> node, which must be a direct child or a direct reference of the <see cref="parentNode"/>.
        /// </summary>
        /// <param name="parentPath">The <see cref="ModelNodePath"/> corresponding to <see cref="parentNode"/>.</param>
        /// <param name="parentNode">The parent node which must be a direct child or a direct reference of the <see cref="parentNode"/>.</param>
        /// <param name="target">The target node for which to build a <see cref="ModelNodePath"/> instance.</param>
        /// <returns></returns>
        public static ModelNodePath GetChildPath(ModelNodePath parentPath, IModelNode parentNode, IModelNode target)
        {
            var result = GetNextPath(parentPath, parentNode, target);
            if (result != null)
            {
                result.targetIsRootNode = result.RootNode == target;
            }
            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsValid ? "(root)" + path.Select(x => x.ToString()).Aggregate((current, next) => current + next) : "(invalid)";
        }

        private ModelNodePath Clone()
        {
            var clone = new ModelNodePath { RootNode = RootNode, targetIsRootNode = targetIsRootNode };
            clone.path.AddRange(path);
            return clone;
        }

        private static ModelNodePath GetNextPath(ModelNodePath parentPath, IModelNode parentNode, IModelNode target)
        {
            var result = parentPath.Clone();
            if (parentNode == target)
                return result;

            var member = parentNode.Children.FirstOrDefault(x => x == target);
            if (member != null)
            {
                // The target is a direct member of the ModelNode
                result.path.Add(new NodePathItemMember { Name = member.Name });
                return result;
            }
            var objectReference = parentNode.Content.Reference as ObjectReference;
            if (objectReference != null && objectReference.TargetNode == target)
            {
                result.path.Add(new NodePathItemTarget());
                return result;
            }

            member = parentNode.Children.FirstOrDefault(x => x.Content.Reference is ObjectReference && ((ObjectReference)x.Content.Reference).TargetNode == target);
            if (member != null)
            {
                result.path.Add(new NodePathItemMember { Name = member.Name });
                result.path.Add(new NodePathItemTarget());
                return result;
            }

            var enumerableReference = parentNode.Content.Reference as ReferenceEnumerable;
            if (enumerableReference != null)
            {
                ObjectReference reference = enumerableReference.FirstOrDefault(x => x.TargetNode == target);
                if (reference != null)
                {
                    result.path.Add(new NodePathItemIndex { Value = reference.Index });
                    return result;
                }
            }
            
            foreach (var child in parentNode.Children)
            {
                enumerableReference = child.Content.Reference as ReferenceEnumerable;
                if (enumerableReference != null)
                {
                    ObjectReference reference = enumerableReference.FirstOrDefault(x => x.TargetNode == target);
                    if (reference != null)
                    {
                        result.path.Add(new NodePathItemMember { Name = child.Name });
                        result.path.Add(new NodePathItemIndex { Value = reference.Index });
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
