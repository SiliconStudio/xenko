using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A class that is capable of visiting two object hierarchies and "link" corresponding nodes together.
    /// </summary>
    /// <remarks>
    /// One of the two hierarchies is considered to be the "source" for the linker. This hierarchy is visited by the <see cref="GraphNodeLinker"/>,
    /// and for each node, it will try to find a corresponding node in the other "target" hierarchy.
    /// By deriving this class, the way this correspondance between two nodes is established can be customized.
    /// </remarks>
    public class GraphNodeLinker
    {
        private sealed class GraphNodeLinkerVisitor : GraphVisitorBase
        {
            private readonly GraphNodeLinker linker;
            internal readonly Dictionary<IGraphNode, IGraphNode> VisitedLinks = new Dictionary<IGraphNode, IGraphNode>();

            public GraphNodeLinkerVisitor(GraphNodeLinker linker)
            {
                this.linker = linker;
            }

            public void Reset(IGraphNode sourceNode, IGraphNode targetNode)
            {
                VisitedLinks.Clear();
                VisitedLinks.Add(sourceNode, targetNode);
            }

            protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
            {
                var targetNode = linker.FindTarget(node);
                // Override the target node, in case FindTarget returned a different one.
                VisitedLinks[node] = targetNode;
                linker.LinkNodes(node, targetNode);
                base.VisitNode(node, currentPath);
            }

            public override void VisitChildren(IGraphNode node, GraphNodePath currentPath)
            {
                IGraphNode targetNodeParent;
                if (VisitedLinks.TryGetValue(node, out targetNodeParent))
                {
                    foreach (var child in node.Children)
                    {
                        if (ShouldVisitNode(child.Content as MemberContent, child))
                        {
                            VisitedLinks.Add(child, targetNodeParent?.GetChild(child.Name));
                        }
                    }
                }
                base.VisitChildren(node, currentPath);
            }

            protected override void VisitReference(IGraphNode referencer, ObjectReference reference, GraphNodePath targetPath)
            {
                if (ShouldVisitNode(referencer.Content as MemberContent, reference.TargetNode))
                {
                    if (reference.TargetNode != null)
                    {
                        // Prevent re-entrancy in the same object
                        if (VisitedLinks.ContainsKey(reference.TargetNode))
                            return;

                        IGraphNode targetNode;
                        if (VisitedLinks.TryGetValue(referencer, out targetNode))
                        {
                            ObjectReference targetReference = null;
                            if (targetNode != null)
                                targetReference = linker.FindTargetReference(referencer, targetNode, reference);

                            VisitedLinks.Add(reference.TargetNode, targetReference?.TargetNode);
                        }
                    }
                    base.VisitReference(referencer, reference, targetPath);
                }
            }
        }

        private readonly GraphNodeLinkerVisitor visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeLinker"/> class.
        /// </summary>
        public GraphNodeLinker()
        {
            visitor = new GraphNodeLinkerVisitor(this) { ShouldVisit = ShouldVisitSourceNode };
        }

        /// <summary>
        /// Gets or sets the action to execute when two nodes should be linked.
        /// </summary>
        public Action<IGraphNode, IGraphNode> LinkAction { get; set; }

        /// <summary>
        /// Visits and links the node of two different object hierarchies.
        /// </summary>
        /// <param name="sourceNode">The root node of the "source" object to link.</param>
        /// <param name="targetNode">The root node of the "target" object to link.</param>
        public void LinkGraph(IGraphNode sourceNode, IGraphNode targetNode)
        {
            visitor.Reset(sourceNode, targetNode);
            visitor.Visit(sourceNode);
        }

        /// <summary>
        /// Indicates whether the linker should visit the given source node.
        /// </summary>
        /// <param name="memberContent">The member content referencing the source node to evaluate.</param>
        /// <param name="targetNode">The source node to evaluate. Can be the node holding the <paramref name="memberContent"/>, or one of its target node if this node contains a reference.</param>
        /// <returns>True if the node should be visited, false otherwise.</returns>
        protected virtual bool ShouldVisitSourceNode(MemberContent memberContent, IGraphNode targetNode)
        {
            return true;
        }

        /// <summary>
        /// Links a single node of the "source" hierarchy to a node of the "target" hierarchy.
        /// </summary>
        /// <param name="sourceNode">The node from the source hierarchy. Cannot be null.</param>
        /// <param name="targetNode">The node from the target hierarchy. Can be null.</param>
        /// <exception cref="ArgumentNullException">The source node is null.</exception>
        /// <remarks>The default implementation will simply invoke <see cref="LinkAction"/>.</remarks>
        protected virtual void LinkNodes(IGraphNode sourceNode, IGraphNode targetNode)
        {
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            LinkAction?.Invoke(sourceNode, targetNode);
        }

        /// <summary>
        /// Finds the target of a given source node.
        /// </summary>
        /// <param name="sourceNode">The source node for which to find a target node. Cannot be null.</param>
        /// <returns>A node in the target hierarchy that corresponds to the given node.</returns>
        /// <remarks>
        /// The default implementation looks for node with the same names and or the same types of reference.
        /// This method can return null if there is no matching node in the target hierarchy.
        /// </remarks>
        protected virtual IGraphNode FindTarget(IGraphNode sourceNode)
        {
            IGraphNode targetNode;
            return visitor.VisitedLinks.TryGetValue(sourceNode, out targetNode) ? targetNode : null;
        }

        /// <summary>
        /// Finds a reference in a target node to correspond with the given reference from the source node.
        /// </summary>
        /// <param name="sourceNode">The source node that contains the reference.</param>
        /// <param name="targetNode">The target node in which to look for a matching reference.</param>
        /// <param name="sourceReference">The reference in the source node for which to look for a correspondance in the target node.</param>
        /// <returns>A reference of the target node corresponding to the given reference in the source node, or null if there is no match.</returns>
        /// <remarks>
        /// The source reference can either be directly the <see cref="IContent.Reference"/> of the source node if this reference is
        /// an <see cref="ObjectReference"/>, or one of the reference contained inside <see cref="IContent.Reference"/> if this reference
        /// is a <see cref="ReferenceEnumerable"/>. The <see cref="IReference.Index"/> property indicates the index of the reference in this case.
        /// The default implementation returns a reference in the target node that matches the index of the source reference, if available.
        /// </remarks>
        protected virtual ObjectReference FindTargetReference(IGraphNode sourceNode, IGraphNode targetNode, ObjectReference sourceReference)
        {
            if (sourceReference.Index.IsEmpty)
                return targetNode.Content.Reference as ObjectReference;

            var targetReference = targetNode.Content.Reference as ReferenceEnumerable;
            return targetReference?[sourceReference.Index];
        }
    }
}
