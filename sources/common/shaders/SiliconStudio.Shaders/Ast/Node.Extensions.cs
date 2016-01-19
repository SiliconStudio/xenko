// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;

namespace SiliconStudio.Shaders.Ast
{
    /// <summary>
    /// Extensions for <see cref="Node"/>.
    /// </summary>
    public static class NodeExtensions
    {
        /// <summary>
        /// Get descendants for the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>An enumeration of descendants</returns>
        private static IEnumerable<Node> DescendantsImpl(this Node node)
        {
            if (node != null)
            {
                yield return node;

                foreach (var children in node.Childrens())
                {
                    if (children != null)
                        foreach (var descendant in children.Descendants())
                        {
                            yield return descendant;
                        }
                }
            }
        }

        /// <summary>
        /// Get descendants for the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>An enumeration of descendants</returns>
        public static IEnumerable<Node> Descendants(this Node node)
        {
            if (node != null)
            {
                foreach (var children in node.Childrens())
                {
                    if (children != null)
                        foreach (var descendant in children.DescendantsImpl())
                        {
                            yield return descendant;
                        }
                }
            }
        }
    }
}
