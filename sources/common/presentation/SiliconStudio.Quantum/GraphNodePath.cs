// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A class describing the path of a node, relative to a root node. The path can cross references, array, etc.
    /// </summary>
    /// <remarks>This class is immutable.</remarks>
    public class GraphNodePath
    {
        /// <summary>
        /// An enum that describes the type of an item of a model node path.
        /// </summary>
        public enum ElementType
        {
            /// <summary>
            /// This item is a member (child) of the previous node
            /// </summary>
            Member,
            /// <summary>
            /// This item is the target of the object reference of the previous node.
            /// </summary>
            Target,
            /// <summary>
            /// This item is the target of a enumerable reference of the previous node corresponding to the associated index.
            /// </summary>
            Index,
        }

        private class NodePathElement
        {
            public ElementType Type;
            public object Value;

            public override string ToString()
            {
                switch (Type)
                {
                    case ElementType.Member:
                        return $".{Value}";
                    case ElementType.Target:
                        return "-> (Target)";
                    case ElementType.Index:
                        return $"[{Value}]";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private readonly List<NodePathElement> path = new List<NodePathElement>();

        private GraphNodePath(IGraphNode rootNode, bool isEmpty)
        {
            RootNode = rootNode;
            IsEmpty = isEmpty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodePath"/> with the given root node.
        /// </summary>
        /// <param name="rootNode">The root node to represent with this instance of <see cref="GraphNodePath"/>.</param>
        public GraphNodePath(IGraphNode rootNode)
            : this(rootNode, true)
        {
        }

        /// <summary>
        /// Gets the root node of this path.
        /// </summary>
        public IGraphNode RootNode { get; }

        /// <summary>
        /// Gets whether this path is a valid path.
        /// </summary>
        public bool IsValid => path.Count > 0 || IsEmpty;

        /// <summary>
        /// Gets whether this path is empty.
        /// </summary>
        /// <remarks>An empty path resolves to <see cref="RootNode"/>.</remarks>
        public bool IsEmpty { get; }

        /// <summary>
        /// Gets the source node corresponding to this path.
        /// </summary>
        /// <param name="targetIndex">The index to the target node, if applicable.</param>
        /// <returns>The node corresponding to this path.</returns>
        /// <exception cref="InvalidOperationException">The path is invalid.</exception>
        public IGraphNode GetSourceNode(out Index targetIndex)
        {
            if (!IsValid)
                throw new InvalidOperationException("The node path is invalid.");

            IGraphNode node = RootNode;
            targetIndex = Index.Empty;
            foreach (var itemPath in path)
            {
                targetIndex = Index.Empty;
                switch (itemPath.Type)
                {
                    case ElementType.Member:
                        var name = (string)itemPath.Value;
                        node = node.Children.Single(x => x.Name == name);
                        break;
                    case ElementType.Target:
                        if (itemPath != path[path.Count - 1])
                        {
                            var objectRefererence = (ObjectReference)node.Content.Reference;
                            node = objectRefererence.TargetNode;
                        }
                        break;
                    case ElementType.Index:
                        if (itemPath != path[path.Count - 1])
                        {
                            var enumerableReference = (ReferenceEnumerable)node.Content.Reference;
                            var objectRefererence = enumerableReference.Single(x => Equals(x.Index, itemPath.Value));
                            node = objectRefererence.TargetNode;
                        }
                        targetIndex = (Index)itemPath.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return node;
        }
        
        /// <summary>
        /// Appends an elemnt to this path a <see cref="GraphNodePath"/> corresponding to the given <see cref="target"/> node, which must be a direct child or a direct reference of the <see cref="parentNode"/>.
        /// </summary>
        /// <param name="parentNode">The parent node which must be a direct child or a direct reference of the <see cref="target"/>.</param>
        /// <param name="target">The target node for which to build a <see cref="GraphNodePath"/> instance.</param>
        /// <param name="type">The type of child to append.</param>
        /// <param name="index">The index of the target if it is in an enumerable reference.</param>
        /// <returns></returns>
        public GraphNodePath Append(IGraphNode parentNode, IGraphNode target, ElementType type, object index)
        {
            if (parentNode == target)
                return Clone();

            var result = Clone(RootNode, false);

            switch (type)
            {
                case ElementType.Member:
                    result.path.Add(new NodePathElement { Type = ElementType.Member, Value = target.Name });
                    return result;
                case ElementType.Target:
                    result.path.Add(new NodePathElement { Type = ElementType.Target });
                    return result;
                case ElementType.Index:
                    result.path.Add(new NodePathElement { Type = ElementType.Index, Value = index });
                    return result;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsValid ? "(root)" + path.Select(x => x.ToString()).Aggregate((current, next) => current + next) : "(invalid)";
        }

        public GraphNodePath Clone(IGraphNode newRoot)
        {
            return Clone(newRoot, IsEmpty);
        }

        public GraphNodePath Clone()
        {
            return Clone(RootNode, IsEmpty);
        }

        // TODO: re-implement each of the method below in an optimized way.

        public GraphNodePath PushMember(string memberName) => PushElement(memberName, ElementType.Member);

        public GraphNodePath PushTarget() => PushElement(null, ElementType.Target);

        public GraphNodePath PushIndex(Index index) => PushElement(index, ElementType.Index);

        private GraphNodePath PushElement(object elementValue, ElementType type)
        {
            var result = Clone();
            switch (type)
            {
                case ElementType.Member:
                    if (!(elementValue is string))
                        throw new ArgumentException("The value must be a string when type is ElementType.Member.");
                    break;
                case ElementType.Target:
                    if (elementValue != null)
                        throw new ArgumentException("The value must be null when type is ElementType.Target.");
                    break;
                case ElementType.Index:
                    if (!(elementValue is Index))
                        throw new ArgumentException("The value must be an Index when type is ElementType.Index.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
            result.path.Add(new NodePathElement { Type = type, Value = elementValue });
            return result;
        }

        public GraphNodePath SubPath(int nodeCount)
        {
            if (nodeCount < 0 || nodeCount >= path.Count)
                throw new ArgumentOutOfRangeException(nameof(nodeCount));

            var subPath = new GraphNodePath(RootNode);
            subPath.path.AddRange(path.Take(nodeCount));
            return subPath;
        }

        public MemberPath ToMemberPath()
        {
            if (!IsValid)
                throw new InvalidOperationException("The node path is invalid.");

            var memberPath = new MemberPath();
            var node = RootNode;
            foreach (var itemPath in path)
            {
                switch (itemPath.Type)
                {
                    case ElementType.Member:
                        var name = (string)itemPath.Value;
                        node = node.Children.Single(x => x.Name == name);
                        memberPath.Push(((MemberContent)node.Content).Member);
                        break;
                    case ElementType.Target:
                        if (itemPath != path[path.Count - 1])
                        {
                            var objectRefererence = (ObjectReference)node.Content.Reference;
                            node = objectRefererence.TargetNode;
                        }
                        break;
                    case ElementType.Index:
                        if (itemPath != path[path.Count - 1])
                        {
                            var enumerableReference = (ReferenceEnumerable)node.Content.Reference;
                            var descriptor = node.Content.Descriptor;
                            var collectionDescriptor = descriptor as CollectionDescriptor;
                            if (collectionDescriptor != null)
                                memberPath.Push(collectionDescriptor, (int)itemPath.Value);
                            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
                            if (dictionaryDescriptor != null)
                                memberPath.Push(dictionaryDescriptor, itemPath.Value);

                            var objectRefererence = enumerableReference.Single(x => Equals(x.Index, itemPath.Value));
                            node = objectRefererence.TargetNode;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return memberPath;
        }

        private GraphNodePath Clone(IGraphNode newRoot, bool isEmpty)
        {
            var clone = new GraphNodePath(newRoot, isEmpty);
            clone.path.AddRange(path);
            return clone;
        }
    }
}
