using System;

namespace SiliconStudio.Quantum
{
    public static class GraphNodeExtensions
    {
        public static void RegisterChanging(this IGraphNode node, Action<object, INodeChangeEventArgs> handler)
        {
            var memberNode = node as IMemberNode;
            if (memberNode != null)
            {
                var eventHandler = new EventHandler<MemberNodeChangeEventArgs>(handler);
                memberNode.Changing += eventHandler;
            }
            var objectNode = node as IObjectNode;
            if (objectNode != null)
            {
                var eventHandler = new EventHandler<ItemChangeEventArgs>(handler);
                objectNode.ItemChanging += eventHandler;
            }
        }

        public static void RegisterChanged(this IGraphNode node, Action<object, INodeChangeEventArgs> handler)
        {
            var memberNode = node as IMemberNode;
            if (memberNode != null)
            {
                var eventHandler = new EventHandler<MemberNodeChangeEventArgs>(handler);
                memberNode.Changed += eventHandler;
            }
            var objectNode = node as IObjectNode;
            if (objectNode != null)
            {
                var eventHandler = new EventHandler<ItemChangeEventArgs>(handler);
                objectNode.ItemChanged += eventHandler;
            }
        }

        public static void UnregisterChanging(this IGraphNode node, Action<object, INodeChangeEventArgs> handler)
        {
            var memberNode = node as IMemberNode;
            if (memberNode != null)
            {
                var eventHandler = new EventHandler<MemberNodeChangeEventArgs>(handler);
                memberNode.Changing -= eventHandler;
            }
            var objectNode = node as IObjectNode;
            if (objectNode != null)
            {
                var eventHandler = new EventHandler<ItemChangeEventArgs>(handler);
                objectNode.ItemChanging -= eventHandler;
            }
        }

        public static void UnregisterChanged(this IGraphNode node, Action<object, INodeChangeEventArgs> handler)
        {
            var memberNode = node as IMemberNode;
            if (memberNode != null)
            {
                var eventHandler = new EventHandler<MemberNodeChangeEventArgs>(handler);
                memberNode.Changed -= eventHandler;
            }
            var objectNode = node as IObjectNode;
            if (objectNode != null)
            {
                var eventHandler = new EventHandler<ItemChangeEventArgs>(handler);
                objectNode.ItemChanged -= eventHandler;
            }
        }
    }
}
