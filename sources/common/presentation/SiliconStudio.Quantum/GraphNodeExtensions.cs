// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

namespace SiliconStudio.Quantum
{
    public static class GraphNodeExtensions
    {
        /// <summary>
        /// Retrieves the child node of the given <see cref="IGraphNode"/> that matches the given name.
        /// </summary>
        /// <param name="graphNode">The graph node to look into.</param>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        public static IGraphNode GetChild(this IGraphNode graphNode, string name)
        {
            return graphNode.Children.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Retrieves the target node of the given <see cref="IGraphNode"/>.
        /// </summary>
        /// <param name="graphNode">The graph node to look into.</param>
        /// <returns>The target node of the given <see cref="IGraphNode"/>.</returns>
        public static IGraphNode GetTarget(this IGraphNode graphNode)
        {
            return graphNode.Content.Reference.AsObject.TargetNode;
        }

        /// <summary>
        /// Retrieves the target node of the given <see cref="IGraphNode"/> at the given index.
        /// </summary>
        /// <param name="graphNode">The graph node to look into.</param>
        /// <param name="index">The index to look into.</param>
        /// <returns>The target node of the given <see cref="IGraphNode"/>.</returns>
        public static IGraphNode GetTarget(this IGraphNode graphNode, Index index)
        {
            return graphNode.Content.Reference.AsEnumerable[index].TargetNode;
        }
    }
}
