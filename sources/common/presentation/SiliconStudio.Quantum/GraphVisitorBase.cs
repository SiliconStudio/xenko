using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// A class that allows to visit a hierarchy of <see cref="IGraphNode"/>, going through children and references.
    /// </summary>
    public class GraphVisitorBase
    {
        private readonly HashSet<IGraphNode> visitedNodes = new HashSet<IGraphNode>();

        /// <summary>
        /// Gets or sets whether to skip the root node passed to <see cref="Visit"/> when raising the <see cref="Visiting"/> event.
        /// </summary>
        public bool SkipRootNode { get; set; }

        /// <summary>
        /// Gets or sets a method that will be invoked to check whether a node should be visited or not.
        /// </summary>
        public Func<MemberContent, IGraphNode, bool> ShouldVisit { get; set; }

        /// <summary>
        /// Gets the root node of the current visit.
        /// </summary>
        protected IGraphNode RootNode { get; private set; }

        /// <summary>
        /// Raised when a node is visited.
        /// </summary>
        public event Action<IGraphNode, GraphNodePath> Visiting;

        /// <summary>
        /// Visits a hierarchy of node, starting by the given root node.
        /// </summary>
        /// <param name="node">The root node of the visit</param>
        /// <param name="initialPath">The initial path of the root node, if this visit occurs in the context of a sub-hierarchy. Can be null.</param>
        public virtual void Visit(IGraphNode node, GraphNodePath initialPath = null)
        {
            var path = initialPath ?? new GraphNodePath(node);
            RootNode = node;
            if (ShouldVisitNode(null, node))
            {
                VisitNode(node, path);
            }
            RootNode = null;
        }

        /// <summary>
        /// Visits a single node.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        /// <remarks>This method is in charge of pursuing the visit with the children and references of the given node, as well as raising the <see cref="Visiting"/> event.</remarks>
        protected virtual void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            visitedNodes.Add(node);
            if (node != RootNode || !SkipRootNode)
            {
                Visiting?.Invoke(node, currentPath);
            }
            VisitChildren(node, currentPath);
            VisitSingleTarget(node, currentPath);
            VisitEnumerableTargets(node, currentPath);
            visitedNodes.Remove(node);
        }

        /// <summary>
        /// Visits the children of the given node.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        public virtual void VisitChildren(IGraphNode node, GraphNodePath currentPath)
        {
            foreach (var child in node.Children)
            {
                var childPath = currentPath.PushMember(child.Name);
                if (ShouldVisitNode(child.Content as MemberContent, child))
                {
                    VisitNode(child, childPath);
                }
            }
        }

        /// <summary>
        /// Visits the <see cref="ObjectReference"/> contained in the given node, if any.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        public virtual void VisitSingleTarget(IGraphNode node, GraphNodePath currentPath)
        {
            var objectReference = node.Content.Reference as ObjectReference;
            if (objectReference?.TargetNode != null)
            {
                var targetPath = currentPath.PushTarget();
                VisitReference(node, objectReference, targetPath);
            }
        }

        /// <summary>
        /// Visits the <see cref="ReferenceEnumerable"/> contained in the given node, if any.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        public virtual void VisitEnumerableTargets(IGraphNode node, GraphNodePath currentPath)
        {
            var enumerableReference = node.Content.Reference as ReferenceEnumerable;
            if (enumerableReference != null)
            {
                foreach (var reference in enumerableReference.Where(x => x.TargetNode != null))
                {
                    var targetPath = currentPath.PushIndex(reference.Index);
                    VisitReference(node, reference, targetPath);
                }
            }
        }

        /// <summary>
        /// Visits an <see cref="ObjectReference"/>.
        /// </summary>
        /// <param name="referencer">The node containing the reference to visit.</param>
        /// <param name="reference">The reference to visit.</param>
        /// <param name="targetPath">The path of the node targeted by this reference.</param>
        protected virtual void VisitReference(IGraphNode referencer, ObjectReference reference, GraphNodePath targetPath)
        {
            if (ShouldVisitNode(referencer.Content as MemberContent, reference.TargetNode))
            {
                VisitNode(reference.TargetNode, targetPath);
            }
        }

        /// <summary>
        /// Indicates whether a node should be visited.
        /// </summary>
        /// <param name="memberContent">The member content referencing the node to evaluate.</param>
        /// <param name="targetNode">The node to evaluate. Can be the node holding the <paramref name="memberContent"/>, or one of its target node if this node contains a reference.</param>
        /// <returns>True if the node should be visited, False otherwise.</returns>
        protected virtual bool ShouldVisitNode(MemberContent memberContent, IGraphNode targetNode)
        {
            return !visitedNodes.Contains(targetNode) && (ShouldVisit?.Invoke(memberContent, targetNode) ?? true);
        }
    }
}
