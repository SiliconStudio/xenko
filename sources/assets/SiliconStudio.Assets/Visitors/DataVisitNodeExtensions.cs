// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// Extensions for <see cref="IDataVisitNode{T}"/>
    /// </summary>
    public static class DataVisitNodeExtensions
    {
        /// <summary>
        /// Counts recursively the number of nodes, including the specified node.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <param name="node">The node.</param>
        /// <returns>The number of nodes.</returns>
        public static int CountChildren<T>(this T node) where T : class, IDataVisitNode<T>
        {
            return node.Children(item => true).Count();
        }

        /// <summary>
        /// Iterate on a <see cref="IDataVisitNode{T}" /> recursively.
        /// </summary>
        /// <typeparam name="T">Type of the node</typeparam>
        /// <param name="node">The node.</param>
        /// <param name="acceptNode">A visitor to accept the node or not in the returning iteration.</param>
        /// <param name="shouldVisitChildren">A visitor to indicate wether or not to visit children (members and items).</param>
        /// <returns>An enumeration on nodes.</returns>
        /// <exception cref="System.ArgumentNullException">acceptNode</exception>
        public static IEnumerable<T> Children<T>(this T node, Func<T, bool> acceptNode, Func<T, bool> shouldVisitChildren = null) where T : class, IDataVisitNode<T>
        {
            if (acceptNode == null) throw new ArgumentNullException("acceptNode");

            if (node == null)
            {
                yield break;
            }

            if (shouldVisitChildren != null && !shouldVisitChildren(node))
            {
                yield break;
            }

            if (acceptNode(node))
            {
                yield return node;
            }

            if (node.HasMembers)
            {
                foreach (var diffMember in node.Members)
                {
                    foreach (var sub in diffMember.Children(acceptNode, shouldVisitChildren))
                    {
                        yield return sub;
                    }
                }
            }

            if (node.HasItems)
            {
                foreach (var diffItem in node.Items)
                {
                    foreach (var sub in diffItem.Children(acceptNode, shouldVisitChildren))
                    {
                        yield return sub;
                    }
                }
            }
        }

        /// <summary>
        /// Dumps a <see cref="IDataVisitNode{T}"/> recursively to a writer, used for debug purposes.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="level">The initial level of indent. Default to 0</param>
        /// <exception cref="System.ArgumentNullException">writer</exception>
        public static void Dump<T>(this T node, TextWriter writer, int level = 0) where T : class, IDataVisitNode<T>
        {
            if (writer == null) throw new ArgumentNullException("writer");
            writer.WriteLine("{0}- {1}", string.Concat(Enumerable.Repeat("    ", level)), node);
            level++;
            if (node.HasMembers)
            {
                foreach (var diffMember in node.Members)
                {
                    diffMember.Dump(writer, level);
                }
            }
            if (node.HasMembers && node.HasItems)
                writer.WriteLine("{0}- Items:", string.Concat(Enumerable.Repeat("    ", level)));

            if (node.HasItems)
            {
                foreach (var diffItem in node.Items)
                {
                    diffItem.Dump(writer, level);
                }
            }
        }

        public static string Dump<T>(this T node) where T : class, IDataVisitNode<T>
        {
            var stringWriter = new StringWriter();
            Dump(node, stringWriter);
            return stringWriter.ToString();
        }
    }
}