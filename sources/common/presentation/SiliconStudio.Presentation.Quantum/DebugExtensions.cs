// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace SiliconStudio.Presentation.Quantum
{
    public static class DebugExtensions
    {
        public static void AssertHierarchy(this IObservableNode node)
        {
            foreach (var child in node.Children)
            {
                if (child.Parent != node)
                    throw new Exception("Parent/Children mismatch");
                AssertHierarchy(child);
            }
        }

        [Pure]
        public static string PrintHierarchy(this IObservableNode node, int indentation = 0)
        {
            var builder = new StringBuilder();
            PrintHierarchyInternal(node, 0, builder);
            return builder.ToString();
        }

        private static void PrintHierarchyInternal(IObservableNode node, int indentation, StringBuilder builder)
        {
            PrintIndentation(indentation, builder);
            builder.Append(node.Name ?? "<untitled>");
            if (!node.Index.IsEmpty)
            {
                builder.Append("[");
                builder.Append(node.Index);
                builder.Append("]");
            }
            builder.Append(": [");
            builder.Append(node.Type.Name);
            builder.Append("] = ");
            builder.Append(node.Value?.ToString().Replace(Environment.NewLine, " ") ?? "(null)");

            if (node.Commands.Any())
            {
                builder.Append("Cmd: ");
                foreach (var command in node.Commands)
                {
                    builder.Append("(");
                    builder.Append(((NodeCommandWrapperBase)command).Name);
                    builder.Append(")");
                }
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
