using System;

namespace SiliconStudio.Quantum.Tests
{
    public static class Helper
    {
        public static void PrintModelContainerContent(ModelContainer container, IModelNode rootNode = null)
        {
            Console.WriteLine(@"Container content:");
            Console.WriteLine(@"------------------");
            // Print the root node first, if specified
            if (rootNode != null)
                Console.WriteLine(rootNode.PrintHierarchy());

            // Print other nodes next
            // TODO: FIXME
            //foreach (var node in container.Guids.Select(container.GetModelNode).Where(x => x != rootNode))
            //{
            //    Console.WriteLine(node.PrintHierarchy());
            //}
            Console.WriteLine(@"------------------");
        }

        public static void ConsistencyCheck(ModelContainer container, object rootObject)
        {
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            var model = container.GetModelNode(rootObject);
            visitor.Check((ModelNode)model, rootObject, rootObject.GetType(), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }
    }
}