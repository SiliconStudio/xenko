// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Tests
{
    public static class Helper
    {
        public static void PrintModelContainerContent(NodeContainer container, IGraphNode rootNode = null)
        {
            Console.WriteLine(@"Container content:");
            Console.WriteLine(@"------------------");
            // Print the root node first, if specified
            if (rootNode != null)
                Console.WriteLine(rootNode.PrintHierarchy());

            // Print other nodes next
            // TODO: FIXME
            //foreach (var node in container.Guids.Select(container.GetNode).Where(x => x != rootNode))
            //{
            //    Console.WriteLine(node.PrintHierarchy());
            //}
            Console.WriteLine(@"------------------");
        }

        public static void ConsistencyCheck(NodeContainer container, object rootObject)
        {
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            var model = container.GetNode(rootObject);
            visitor.Check((GraphNode)model, rootObject, rootObject.GetType(), true);
            foreach (var node in container.Nodes)
            {
                visitor.Check((GraphNode)node, node.Content.Value, node.Content.Type, true);
            }
        }
    }
}