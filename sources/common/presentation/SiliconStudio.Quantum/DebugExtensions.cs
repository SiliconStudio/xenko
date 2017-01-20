// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Text;

namespace SiliconStudio.Quantum
{
    public static class DebugExtensions
    {
        public static string PrintHierarchy(this IContentNode node)
        {
            var builder = new StringBuilder();
            PrintHierarchyInternal(node, 0, builder);
            return builder.ToString();
        }

        private static void PrintHierarchyInternal(IContentNode node, int indentation, StringBuilder builder)
        {
            PrintIndentation(indentation, builder);
            builder.Append(node.Guid + " ");
            PrintIndentation(indentation, builder);
            builder.Append((node as IMemberNode)?.Name ?? node.Type.Name);
            builder.Append(": [");
            builder.Append(node.GetType().Name);
            builder.Append("] = ");
            if (node.IsReference)
            {
                if (node.Value != null)
                {
                    builder.Append(node.Value.ToString().Replace(Environment.NewLine, " "));
                    builder.Append(" > ");
                }
                builder.Append("Reference -> ");
                //builder.Append(node.Reference);
            }
            else if (node.Value == null)
            {
                builder.Append("(null)");
            }
            else
            {
                builder.Append(node.Value.ToString().Replace(Environment.NewLine, " "));
            }
            builder.AppendLine();
            var objNode = node as IObjectNode;
            if (objNode != null)
            {
                foreach (var child in objNode.Members)
                {
                    PrintHierarchyInternal(child, indentation + 4, builder);
                }
            }
        }

        private static void PrintIndentation(int indendation, StringBuilder builder)
        {
            for (int i = 0; i < indendation; ++i)
                builder.Append(' ');
        }
    }
}
