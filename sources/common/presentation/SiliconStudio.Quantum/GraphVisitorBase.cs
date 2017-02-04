using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
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
        internal Func<IMemberNode, IGraphNode, bool> ShouldVisit { get; set; }

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
        /// <param name="memberNode">The member content containing the node to visit, if relevant. This is used to properly check if the root node should be visited.</param>
        /// <param name="initialPath">The initial path of the root node, if this visit occurs in the context of a sub-hierarchy. Can be null.</param>
        public virtual void Visit([NotNull] IGraphNode node, [CanBeNull] MemberNode memberNode = null, [CanBeNull] GraphNodePath initialPath = null)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            var path = initialPath ?? new GraphNodePath(node);
            RootNode = node;
            if (ShouldVisitNode(memberNode, node))
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
        protected virtual void VisitNode([NotNull] IGraphNode node, [NotNull] GraphNodePath currentPath)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (currentPath == null) throw new ArgumentNullException(nameof(currentPath));
            visitedNodes.Add(node);
            if (node != RootNode || !SkipRootNode)
            {
                Visiting?.Invoke(node, currentPath);
            }
            var objectNode = node as IObjectNode;
            if (objectNode != null)
            {
                VisitChildren(objectNode, currentPath);
                VisitItemTargets(objectNode, currentPath);
            }
            var memberNode = node as IMemberNode;
            if (memberNode != null)
            {
                VisitMemberTarget(memberNode, currentPath);
            }
            visitedNodes.Remove(node);
        }

        /// <summary>
        /// Visits the children of the given node.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        protected virtual void VisitChildren([NotNull] IObjectNode node, [NotNull] GraphNodePath currentPath)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (currentPath == null) throw new ArgumentNullException(nameof(currentPath));
            foreach (var child in node.Members)
            {
                var childPath = currentPath.PushMember(child.Name);
                if (ShouldVisitNode(child, child))
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
        protected virtual void VisitMemberTarget([NotNull] IMemberNode node, [NotNull] GraphNodePath currentPath)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (currentPath == null) throw new ArgumentNullException(nameof(currentPath));
            if (node.TargetReference?.TargetNode != null)
            {
                var targetPath = currentPath.PushTarget();
                VisitReference(node, node.TargetReference, targetPath);
            }
        }

        /// <summary>
        /// Visits the <see cref="ReferenceEnumerable"/> contained in the given node, if any.
        /// </summary>
        /// <param name="node">The node being visited.</param>
        /// <param name="currentPath">The path of the node being visited.</param>
        public virtual void VisitItemTargets([NotNull] IObjectNode node, [NotNull] GraphNodePath currentPath)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (currentPath == null) throw new ArgumentNullException(nameof(currentPath));
            var enumerableReference = node.ItemReferences;
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
        protected virtual void VisitReference([NotNull] IGraphNode referencer, [NotNull] ObjectReference reference, [NotNull] GraphNodePath targetPath)
        {
            if (referencer == null) throw new ArgumentNullException(nameof(referencer));
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (targetPath == null) throw new ArgumentNullException(nameof(targetPath));
            if (ShouldVisitNode(referencer as MemberNode, reference.TargetNode))
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
        protected virtual bool ShouldVisitNode(IMemberNode memberContent, IGraphNode targetNode)
        {
            return !visitedNodes.Contains(targetNode) && (ShouldVisit?.Invoke(memberContent, targetNode) ?? true);
        }
    }
}
