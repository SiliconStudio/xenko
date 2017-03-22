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
        public static void AssertHierarchy(this INodeViewModel node)
        {
            foreach (var child in node.Children)
            {
                if (child.Parent != node)
                    throw new Exception("Parent/Children mismatch");
                AssertHierarchy(child);
            }
        }
    }
}
