using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
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
        /// Links the graph nodes of two objects together. This method will iterate on the children, references
        /// </summary>
        /// <param name="sourceRootNode"></param>
        /// <param name="targetRootNode"></param>
        /// <param name="linkAction"></param>
        public static void LinkNodes(IGraphNode sourceRootNode, IGraphNode targetRootNode, LinkActionDelegate linkAction)
        {
            if (sourceRootNode == null) throw new ArgumentNullException(nameof(sourceRootNode));
            if (targetRootNode == null) throw new ArgumentNullException(nameof(targetRootNode));
            if (linkAction == null) throw new ArgumentNullException(nameof(linkAction));

            var nodes = new Queue<ContentNodeLink>();
            nodes.Enqueue(new ContentNodeLink(sourceRootNode, targetRootNode));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                linkAction(node.Source, node.Target);
                if (node.Target != null)
                {
                    // Enqueue children
                    foreach (var child in node.Source.Children)
                    {
                        var baseChild = node.Target.Children.FirstOrDefault(x => x.Name == child.Name);
                        if (baseChild != null)
                        {
                            nodes.Enqueue(new ContentNodeLink(child, baseChild));
                        }
                    }
                    // Enqueue object reference
                    var objectReference = node.Source.Content.Reference as ObjectReference;
                    if (objectReference?.TargetNode != null)
                    {
                        var baseObjectReference = node.Target.Content.Reference.AsObject;
                        nodes.Enqueue(new ContentNodeLink(objectReference.TargetNode, baseObjectReference?.TargetNode));
                    }
                    // Enqueue enumerable references
                    var enumReference = node.Source.Content.Reference as ReferenceEnumerable;
                    var baseEnumReference = node.Target.Content.Reference as ReferenceEnumerable;
                    if (enumReference != null && baseEnumReference != null)
                    {
                        foreach (var reference in enumReference.Where(x => x.TargetNode != null))
                        {
                            var baseReference = baseEnumReference.First(x => Equals(reference.Index, x.Index));
                            nodes.Enqueue(new ContentNodeLink(reference.TargetNode, baseReference?.TargetNode));
                        }
                    }
                }
            }
        }
    }
}