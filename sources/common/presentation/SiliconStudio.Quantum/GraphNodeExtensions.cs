// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public static class GraphNodeExtensions
    {
        /// <summary>
        /// Retrieve the child node of the given <see cref="IGraphNode"/> that matches the given name.
        /// </summary>
        /// <param name="modelNode">The view model node to look into.</param>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        public static IGraphNode GetChild(this IGraphNode modelNode, string name)
        {
            return modelNode.Children.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Gets all the children of the given node, the nodes it references, and their children, recursively.
        /// </summary>
        /// <param name="graphNode">The node for which to obtain all children.</param>
        /// <returns>A sequence of node containing all the nodes that are either child or referenced by the given node, recursively.</returns>
        public static IEnumerable<IGraphNode> GetAllChildNodes(this IGraphNode graphNode)
        {
            var processedNodes = new HashSet<IGraphNode>();
            var nodeStack = new Stack<IGraphNode>();
            nodeStack.Push(graphNode);

            while (nodeStack.Count > 0)
            {
                var node = nodeStack.Pop();
                processedNodes.Add(node);
                // We don't want to include the initial node
                if (node != graphNode)
                    yield return node;

                // Add child nodes
                node.Children.ForEach(x => nodeStack.Push(x));

                // Add object reference target node
                var objectReference = node.Content.Reference as ObjectReference;
                if (objectReference?.TargetNode != null)
                    nodeStack.Push(objectReference.TargetNode);

                // Add enumerable reference target nodes
                var enumerableReference = node.Content.Reference as ReferenceEnumerable;
                enumerableReference?.Select(x => x.TargetNode).NotNull().ForEach(x => nodeStack.Push(x));
            }
        }

        /// <summary>
        /// Gets whether a given <see cref="ITypeDescriptor"/> represents a collection or a dictionary of primitive values.
        ///  
        /// </summary>
        /// <param name="descriptor">The type descriptor to check.</param>
        /// <returns><c>true</c> if the given <see cref="ITypeDescriptor"/> represents a collection or a dictionary of primitive values, <c>false</c> otherwise.</returns>
        public static bool IsPrimitiveCollection(this ITypeDescriptor descriptor)
        {
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            Type elementType = null;
            if (collectionDescriptor != null)
            {
                elementType = collectionDescriptor.ElementType;
            }
            else if (dictionaryDescriptor != null)
            {
                elementType = dictionaryDescriptor.ValueType;

            }
            if (elementType != null)
            {
                if (elementType.IsNullable())
                    elementType = Nullable.GetUnderlyingType(elementType);

                return elementType.IsPrimitive || elementType == typeof(string) || elementType.IsEnum;
            }
            return false;
        }
    }
}