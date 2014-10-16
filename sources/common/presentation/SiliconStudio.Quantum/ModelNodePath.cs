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
        /// Gets a new instance of <see cref="ModelNodePath"/> corresponding to the path of the given target node relative to the given root node.
        /// </summary>
        /// <param name="rootNode">The root node of the path.</param>
        /// <param name="target">The target node of the path.</param>
        /// <returns>A new instance of the <see cref="ModelNodePath"/>. This instance may not be valid if no path lead to the target node from the root node.</returns>
        public static ModelNodePath GetPath(IModelNode rootNode, IModelNode target)
        {
            var visitedNode = new HashSet<IModelNode>();
            var result = GetPathRecursive(rootNode, target, visitedNode);
            if (result != null)
            {
                result.RootNode = rootNode;
                result.targetIsRootNode = rootNode == target;
            }
            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsValid ? "(root)" + path.Select(x => x.ToString()).Aggregate((current, next) => current + next) : "(invalid)";
        }

        private void Prepend(object item)
        {
            path.Insert(0, item);
        }

        private static ModelNodePath GetPathRecursive(IModelNode modelNode, IModelNode target, ICollection<IModelNode> visitedNode)
        {
            var member = modelNode.Children.Where(x => !visitedNode.Contains(x)).FirstOrDefault(x => x == target);
            var objectReference = modelNode.Content.Reference as ObjectReference;
            var enumerableReference = modelNode.Content.Reference as ReferenceEnumerable;
            var result = new ModelNodePath();

            visitedNode.Add(modelNode);
            
            if (member != null)
            {
                // The target is a direct member of the ModelNode
                result.path.Add(new NodePathItemMember { Name = member.Name });
            }
            else if (objectReference != null && objectReference.TargetNode != null)
            {
                // The target is the TargetNode of the ObjectReference contained in the ModelNode
                if (objectReference.TargetNode != target)
                    result = GetPathRecursive(objectReference.TargetNode, target, visitedNode);

                if (result.IsValid || target == objectReference.TargetNode)
                {
                    result.Prepend(new NodePathItemTarget());
                }
            }
            else if (enumerableReference != null)
            {
                foreach (ObjectReference reference in enumerableReference.Where(x => x.TargetNode != null))
                {
                    if (target != reference.TargetNode)
                        result = GetPathRecursive(reference.TargetNode, target, visitedNode);
                
                    if (result.IsValid || target == reference.TargetNode)
                    {
                        // The target is the TargetNode of an item of the ReferenceEnumerable contained in the ModelNode
                        result.Prepend(new NodePathItemIndex { Value = reference.Index });
                        break;
                    }
                }
            }
            else
            {
                // The target is not directly accessible. Let's invoke this method recursively on each of the child of the ModelNode
                foreach (var child in modelNode.Children.Where(x => !visitedNode.Contains(x)))
                {
                    result = GetPathRecursive(child, target, visitedNode);
                    if (result.IsValid)
                    {
                        result.Prepend(new NodePathItemMember { Name = child.Name });
                        break;
                    }
                }
            } 
            return result;
        }
    }
}
