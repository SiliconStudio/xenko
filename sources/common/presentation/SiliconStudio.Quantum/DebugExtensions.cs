// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Text;

namespace SiliconStudio.Quantum
{
    public static class DebugExtensions
    {
        public static string PrintHierarchy(this IGraphNode node)
        {
            var builder = new StringBuilder();
            PrintHierarchyInternal(node, 0, builder);
            return builder.ToString();
        }

        private static void PrintHierarchyInternal(IGraphNode node, int indentation, StringBuilder builder)
        {
            PrintIndentation(indentation, builder);
            builder.Append(node.Guid + " ");
            PrintIndentation(indentation, builder);
            builder.Append(node.Name ?? "<untitled>");
            builder.Append(": [");
            builder.Append(node.Content.GetType().Name);
            builder.Append("] = ");
            if (node.Content.IsReference)
            {
                if (node.Content.Value != null)
                {
                    builder.Append(node.Content.Value.ToString().Replace(Environment.NewLine, " "));
                    builder.Append(" > ");
                }
                builder.Append("Reference -> ");
                builder.Append(node.Content.Reference);
            }
            else if (node.Content.Value == null)
            {
                builder.Append("(null)");
            }
            else
            {
                builder.Append(node.Content.Value.ToString().Replace(Environment.NewLine, " "));
            }
            builder.AppendLine();
            foreach (var child in node.Children)
            {
                PrintHierarchyInternal(child, indentation + 4, builder);
            }
        }

        private static void PrintIndentation(int indendation, StringBuilder builder)
        {
            for (int i = 0; i < indendation; ++i)
                builder.Append(' ');
        }
    }
}
