using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public struct GraphItem
    {
        public GraphItem(IGraphNode parent, IGraphNode node, object index)
        {
            Node = node;
            Parent = parent;
            Index = index;
        }

        public IGraphNode Node { get; }

        public IGraphNode Parent { get; }

        public object Index { get; }
    }

    public class GraphVisitorBase
    {
        private readonly HashSet<IGraphNode> visitedNodes = new HashSet<IGraphNode>();

        public event Action<IGraphNode, GraphNodePath> Visiting;

        public virtual void Visit(IGraphNode node)
        {
            var path = new GraphNodePath(node);
            VisitNode(node, path);
        }

        public virtual void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            visitedNodes.Add(node);
            Visiting?.Invoke(node, currentPath);
            VisitChildren(node, currentPath);
            VisitSingleTarget(node, currentPath);
            VisitEnumerableTargets(node, currentPath);
            visitedNodes.Remove(node);
        }

        public virtual void VisitChildren(IGraphNode node, GraphNodePath currentPath)
        {
            foreach (var child in node.Children)
            {
                var childPath = currentPath.PushMember(child.Name);
                if (ShouldVisitNode(child, childPath))
                {
                    VisitNode(child, childPath);
                }
            }
        }

        public virtual void VisitSingleTarget(IGraphNode node, GraphNodePath currentPath)
        {
            var objectReference = node.Content.Reference as ObjectReference;
            if (objectReference?.TargetNode != null)
            {
                var targetPath = currentPath.PushTarget();
                VisitReference(node, objectReference, targetPath);
            }
        }

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

        protected virtual void VisitReference(IGraphNode referencer, ObjectReference reference, GraphNodePath targetPath)
        {
            if (ShouldVisitNode(reference.TargetNode, targetPath))
            {
                VisitNode(reference.TargetNode, targetPath);
            }
        }

        protected virtual bool ShouldVisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            return !visitedNodes.Contains(node);
        }
    }

    public class GraphVisitorBaseOld
    {
        private readonly HashSet<IGraphNode> visitedNodes = new HashSet<IGraphNode>();
        private readonly Queue<GraphItem> nodeQueue = new Queue<GraphItem>();

        public void Initialize(IGraphNode rootNode)
        {
            nodeQueue.Enqueue(new GraphItem(null, rootNode, null));
        }

        public void Reset()
        {
            nodeQueue.Clear();
            visitedNodes.Clear();
        }

        public bool VisitNext(out GraphItem nextItem)
        {
            while (true)
            {
                if (nodeQueue.Count == 0)
                {
                    nextItem = new GraphItem(null, null, null);
                    return false;
                }

                nextItem = nodeQueue.Dequeue();
                if (visitedNodes.Add(nextItem.Node))
                    break;
            }

            foreach (var child in nextItem.Node.Children)
            {
                var item = new GraphItem(nextItem.Node, child, null);
                nodeQueue.Enqueue(item);
            }
            var objectReference = nextItem.Node.Content.Reference as ObjectReference;
            var enumerableReference = nextItem.Node.Content.Reference as ReferenceEnumerable;
            if (objectReference?.TargetNode != null)
            {
                var item = new GraphItem(nextItem.Node, objectReference.TargetNode, null);
                nodeQueue.Enqueue(item);
            }
            if (enumerableReference != null)
            {
                foreach (var reference in enumerableReference.Where(x => x.TargetNode != null))
                {
                    var item = new GraphItem(nextItem.Node, reference.TargetNode, reference.Index);
                    nodeQueue.Enqueue(item);
                }
            }
            return true;
        }

        protected virtual bool ShouldVisit(GraphItem item)
        {
            return true;
        }
    }
}
