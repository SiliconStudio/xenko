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

        private struct NodePathElement : IEquatable<NodePathElement>
        {
            public readonly ElementType Type;
            public readonly object Value;

            private NodePathElement(object value, ElementType type)
            {
                Value = value;
                Type = type;
            }

            public static NodePathElement CreateMember(string name)
            {
                return new NodePathElement(name, ElementType.Member);
            }

            public static NodePathElement CreateTarget()
            {
                // We use a guid to allow equality test to fail between two different instances returned by CreateTarget
                return new NodePathElement(Guid.NewGuid(), ElementType.Target);
            }

            public static NodePathElement CreateIndex(Index index)
            {
                // We use a guid to allow equality test to fail between two different instances returned by CreateTarget
                return new NodePathElement(index, ElementType.Index);
            }

            public bool Equals(NodePathElement other)
            {
                return Type == other.Type && Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                return obj is NodePathElement && Equals((NodePathElement)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Type*397) ^ (Value?.GetHashCode() ?? 0);
                }
            }

            public static bool operator ==(NodePathElement left, NodePathElement right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(NodePathElement left, NodePathElement right)
            {
                return !left.Equals(right);
            }

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

        private const int DefaultCapacity = 16;
        private readonly List<NodePathElement> path;

        private GraphNodePath(IGraphNode rootNode, bool isEmpty, int defaultCapacity)
        {
            RootNode = rootNode;
            IsEmpty = isEmpty;
            path = new List<NodePathElement>(defaultCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodePath"/> with the given root node.
        /// </summary>
        /// <param name="rootNode">The root node to represent with this instance of <see cref="GraphNodePath"/>.</param>
        public GraphNodePath(IGraphNode rootNode)
            : this(rootNode, true, DefaultCapacity)
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
                    var name = elementValue as string;
                    if (name == null)
                        throw new ArgumentException("The value must be a non-null string when type is ElementType.Member.");
                    result.path.Add(NodePathElement.CreateMember(name));
                    return result;
                case ElementType.Target:
                    if (elementValue != null)
                        throw new ArgumentException("The value must be null when type is ElementType.Target.");
                    result.path.Add(NodePathElement.CreateTarget());
                    return result;
                case ElementType.Index:
                    if (!(elementValue is Index))
                        throw new ArgumentException("The value must be an Index when type is ElementType.Index.");
                    result.path.Add(NodePathElement.CreateIndex((Index)elementValue));
                    return result;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
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
            var clone = new GraphNodePath(newRoot, isEmpty, Math.Max(path.Count, DefaultCapacity));
            clone.path.AddRange(path);
            return clone;
        }
    }
}
