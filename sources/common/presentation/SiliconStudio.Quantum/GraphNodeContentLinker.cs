using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public class GraphNodeLinker
    {
        private sealed class GraphNodeLinkerVisitor : GraphVisitorBase
        {
            private readonly GraphNodeLinker linker;
            public readonly Dictionary<IGraphNode, IGraphNode> VisitedLinks = new Dictionary<IGraphNode, IGraphNode>();

            public GraphNodeLinkerVisitor(GraphNodeLinker linker)
            {
                this.linker = linker;
            }

            public void Reset(IGraphNode sourceNode, IGraphNode targetNode)
            {
                VisitedLinks.Clear();
                VisitedLinks.Add(sourceNode, targetNode);
            }

            public override void VisitNode(IGraphNode node, GraphNodePath currentPath)
            {
                var targetNode = linker.FindTarget(node);
                linker.LinkNodes(node, targetNode);
                base.VisitNode(node, currentPath);
            }

            public override void VisitChildren(IGraphNode node, GraphNodePath currentPath)
            {
                IGraphNode targetNodeParent;
                if (VisitedLinks.TryGetValue(node, out targetNodeParent) && targetNodeParent != null)
                {
                    foreach (var child in node.Children)
                    {
                        VisitedLinks.Add(child, targetNodeParent.GetChild(child.Name));
                    }
                }
                base.VisitChildren(node, currentPath);
            }

            protected override void VisitReference(IGraphNode referencer, ObjectReference reference, GraphNodePath targetPath)
            {
                if (reference.TargetNode != null)
                {
                    IGraphNode targetNode;
                    if (VisitedLinks.TryGetValue(referencer, out targetNode) && targetNode != null)
                    {
                        var targetReference = linker.FindTargetReference(referencer, targetNode, reference);
                        VisitedLinks.Add(reference.TargetNode, targetReference?.TargetNode);
                    }
                }
                base.VisitReference(referencer, reference, targetPath);
            }
        }

        private readonly GraphNodeLinkerVisitor visitor;

        public GraphNodeLinker()
        {
            visitor = new GraphNodeLinkerVisitor(this);
        }

        public void LinkGraph(IGraphNode sourceNode, IGraphNode targetNode)
        {
            visitor.Reset(sourceNode, targetNode);
            visitor.Visit(sourceNode);
        }

        protected virtual void LinkNodes(IGraphNode sourceNode, IGraphNode targetNode)
        {
            // Do nothing by default
        }

        protected virtual ObjectReference FindTargetReference(IGraphNode sourceNode, IGraphNode targetNode, ObjectReference sourceReference)
        {
            if (sourceReference.Index.IsEmpty)
                return targetNode.Content.Reference as ObjectReference;

            var targetReference = targetNode.Content.Reference as ReferenceEnumerable;
            return targetReference?[sourceReference.Index];
        }

        protected virtual IGraphNode FindTarget(IGraphNode sourceNode)
        {
            IGraphNode targetNode;
            return visitor.VisitedLinks.TryGetValue(sourceNode, out targetNode) ? targetNode : null;
        }
    }

    /// <summary>
    /// A static class providing tools to link graph nodes of two object together.
    /// </summary>
    public static class GraphNodeContentLinker
    {
        /// <summary>
        /// Delegate invoked when linking <see cref="IGraphNode"/> objects together.
        /// </summary>
        /// <param name="sourceNode">The source node of the link.</param>
        /// <param name="targetNode">The target node of the link.</param>
        public delegate void LinkActionDelegate(IGraphNode sourceNode, IGraphNode targetNode);

        public delegate bool ReferenceMatchDelegate(ObjectReference sourceReference, ObjectReference targetReferenceMatch);

        public delegate IGraphNode FindTargetDelegate(IGraphNode sourceNode, IGraphNode currentTarget);

        private struct ContentNodeLink
        {
            public readonly IGraphNode Source;
            public readonly IGraphNode Target;

            public ContentNodeLink(IGraphNode source, IGraphNode target)
            {
                Source = source;
                Target = target;
            }
        }

        /// <summary>
        /// Links the graph nodes of two objects together. This method will iterate on the children and references
        /// </summary>
        /// <param name="sourceRootNode">The root node of the source graph.</param>
        /// <param name="targetRootNode">The root node of the target graph.</param>
        /// <param name="linkAction">The action to invoke to link nodes.</param>
        /// <param name="referenceMatch">A function that checks whether two elements of a collection are actually matching.</param>
        /// <param name="findTarget">An additional function that retrieve a target when the current target node is null.</param>
        public static void LinkNodes(IGraphNode sourceRootNode, IGraphNode targetRootNode, LinkActionDelegate linkAction, ReferenceMatchDelegate referenceMatch = null, FindTargetDelegate findTarget = null)
        {
            if (sourceRootNode == null) throw new ArgumentNullException(nameof(sourceRootNode));
            if (linkAction == null) throw new ArgumentNullException(nameof(linkAction));

            if (referenceMatch == null)
                referenceMatch = (x, y) => Equals(x.Index, y.Index);

            var nodes = new Queue<ContentNodeLink>();
            nodes.Enqueue(new ContentNodeLink(sourceRootNode, targetRootNode));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                if (findTarget != null)
                {
                    var target = findTarget(node.Source, node.Target);
                    if (target != node.Target)
                    {
                        node = new ContentNodeLink(node.Source, target);
                    }
                }
                if (node.Target != null)
                {
                    linkAction(node.Source, node.Target);
                }
                // Enqueue children
                foreach (var sourceChild in node.Source.Children)
                {
                    var targetChild = node.Target?.Children.FirstOrDefault(x => x.Name == sourceChild.Name);
                    nodes.Enqueue(new ContentNodeLink(sourceChild, targetChild));
                }
                // Enqueue object reference
                var sourceObjectReference = node.Source.Content.Reference as ObjectReference;
                if (sourceObjectReference?.TargetNode != null)
                {
                    var targetObjectReference = node.Target?.Content.Reference.AsObject;
                    nodes.Enqueue(new ContentNodeLink(sourceObjectReference.TargetNode, targetObjectReference?.TargetNode));
                }
                // Enqueue enumerable references
                var sourceEnumReference = node.Source.Content.Reference as ReferenceEnumerable;
                var targetEnumReference = node.Target?.Content.Reference as ReferenceEnumerable;
                if (sourceEnumReference != null)
                {
                    foreach (var sourceReference in sourceEnumReference.Where(x => x.TargetNode != null))
                    {
                        var targetReference = targetEnumReference?.FirstOrDefault(x => referenceMatch(x, sourceReference));
                        nodes.Enqueue(new ContentNodeLink(sourceReference.TargetNode, targetReference?.TargetNode));
                    }
                }
            }
        }

    }
}
