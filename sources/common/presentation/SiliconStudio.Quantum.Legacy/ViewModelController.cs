// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// Responsible for creation of view from model and updating values.
    /// </summary>
    // TODO: This class need massive rework once the Quantum refactoring is done
    public static class ViewModelController
    {
        private static readonly SerializerSelector NetworkSerializerSelector;

        public static PropertyKey<ViewModelContext> ContextProperty = new PropertyKey<ViewModelContext>("Context", typeof(ViewModelController));
        public static PropertyKey<IViewModelNode> RootNodeProperty = new PropertyKey<IViewModelNode>("RootNode", typeof(ViewModelController));
        public static PropertyKey<bool> NewItemProperty = new PropertyKey<bool>("NewItem", typeof(ViewModelController));
        public static PropertyKey<int> LastAckPacketIndex = new PropertyKey<int>("LastAckPacketIndex", typeof(ViewModelController));

        static ViewModelController()
        {
            NetworkSerializerSelector = new SerializerSelector();
            NetworkSerializerSelector.RegisterSerializer(new ViewModelNodeSerializer());
        }

        internal static IViewModelNode Combine(IEnumerable<IViewModelNode> modelNodes)
        {
            var nodes = modelNodes as IViewModelNode[] ?? modelNodes.ToArray();
            var contents = nodes.Select(x => x.Content).ToArray();

            IContent combinedContent;
            if (nodes[0].Content.Type == typeof(IList<ViewModelReference>))
            {
                combinedContent = new CombinedListReferenceViewModelContent(contents);
            }
            else if (nodes[0].Content.Type == typeof(ViewModelReference))
            {
                combinedContent = new CombinedReferenceViewModelContent(contents);
            }
            else
            {
                combinedContent = new CombinedViewModelContent(contents);
            }
            var combinedNode = new ViewModelNode(nodes[0].Name, combinedContent);

            if (nodes.Any(x => x.Children.Count != nodes[0].Children.Count))
                throw new InvalidOperationException("Combined nodes should have same children");

            for (int i = 0; i < nodes[0].Children.Count; ++i)
            {
                int index = i;
                var children = nodes.Select(x => x.Children[index]).ToArray();
                if (children.Any(x => x.Name != children[0].Name))
                    throw new InvalidOperationException("Combined nodes should have same children");

                combinedNode.Children.Add(Combine(children));
            }

            return combinedNode;
        }

        private static void SerializeViewModelNode(IViewModelNode viewModelNode, ArchiveMode mode, SerializationStream stream)
        {
            var proxyNode = viewModelNode as ViewModelProxyNode;
            var node = viewModelNode as ViewModelNode;

            if (proxyNode != null)
                stream.Serialize(ref proxyNode, mode);
            else if (node != null)
                stream.Serialize(ref node, mode);
            else
                throw new ArgumentException("viewModelNode type is unsupported.");
        }

        private static void SerializeChildren(IViewModelNode viewModelNode, ArchiveMode mode, bool newItem, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(viewModelNode.Children.Count);
                foreach (var child in viewModelNode.Children)
                {
                    bool isProxy = child is ViewModelProxyNode;
                    stream.Write(isProxy);
                    stream.Write(child.Guid);
                    SerializeViewModelNode(child, ArchiveMode.Serialize, stream);
                }
            }
            else
            {
                var childCount = stream.ReadInt32();
                for (int i = 0; i < childCount; ++i)
                {
                    var isProxy = stream.ReadBoolean();
                    var childGuid = stream.Read<Guid>();

                    IViewModelNode matchingChild;
                    if (newItem)
                    {
                        matchingChild = isProxy ? new ViewModelProxyNode("<Serializing>", null) : new ViewModelNode("<Serializing>", new NullViewModelContent());
                        matchingChild.Guid = childGuid;
                        viewModelNode.Children.Add(matchingChild);
                    }
                    else
                    {
                        matchingChild = viewModelNode.Children.First(x => x.Guid == childGuid);
                    }

                    SerializeViewModelNode(matchingChild, ArchiveMode.Deserialize, stream);
                }
            }
        }

        public class ViewModelNodeSerializer : DataSerializer<ViewModelNode>
        {
            public override void Serialize(ref ViewModelNode viewModelNode, ArchiveMode mode, SerializationStream stream)
            {
                var newItem = stream.Context.Get(NewItemProperty);

                if (mode == ArchiveMode.Serialize)
                {
                    var value = viewModelNode.Content.Value;

                    // Serialize only if Serialize flags is set, and not async
                    bool saveValue = (viewModelNode.Content.SerializeFlags & ViewModelContentSerializeFlags.SerializeValue) == ViewModelContentSerializeFlags.SerializeValue;

                    if (newItem)
                    {
                        stream.Write(viewModelNode.Name != null);
                        if (viewModelNode.Name != null)
                            stream.Write(viewModelNode.Name);

                        var type = saveValue ? viewModelNode.Content.Type : typeof(object);
                        stream.Write(type.AssemblyQualifiedName);
                    }

                    var loadState = viewModelNode.Content.LoadState;

                    if (viewModelNode.Content.LoadState == ViewModelContentState.Loaded
                        && saveValue && (viewModelNode.Content.SerializeFlags & ViewModelContentSerializeFlags.Async) == ViewModelContentSerializeFlags.Async)
                    {
                        viewModelNode.Content.SerializeFlags &= ~(ViewModelContentSerializeFlags.Async | ViewModelContentSerializeFlags.SerializeValue);
                        var viewModelContext = stream.Context.Get(ContextProperty);
                        var rootNode = stream.Context.Get(RootNodeProperty);
                        viewModelContext.PendingAsyncNodes.Enqueue(new KeyValuePair<Guid, IViewModelNode>(rootNode.Guid, viewModelNode));
                        loadState = ViewModelContentState.PendingAsync;
                    }

                    stream.Write(loadState);
                    stream.Write(viewModelNode.Content.Flags);
                    if (loadState == ViewModelContentState.Loaded)
                    {
                        // If content is static, don't serialize it next time
                        if (saveValue && (viewModelNode.Content.SerializeFlags & ViewModelContentSerializeFlags.Static) == ViewModelContentSerializeFlags.Static)
                            viewModelNode.Content.SerializeFlags &= ~(ViewModelContentSerializeFlags.Static | ViewModelContentSerializeFlags.SerializeValue);

                        bool valueUpdated = saveValue && viewModelNode.Content.UpdatedValue != ContentBase.ValueNotUpdated;

                        stream.Write(valueUpdated);

                        if (valueUpdated)
                        {
                            // TODO: TEMPORARY FILTERING!
                            if (viewModelNode.Content.Type == typeof(ExecuteCommand))
                                value = null;

                            stream.SerializeExtended(ref value, mode, null);
                        }
                    }
                }
                else
                {
                    if (newItem)
                    {
                        bool hasPropertyName = stream.ReadBoolean();
                        if (hasPropertyName)
                            viewModelNode.Name = stream.ReadString();

                        var viewModelContentTypeName = stream.ReadString();

                        var viewModelContentType = Type.GetType(viewModelContentTypeName);
                        var viewModelContext = stream.Context.Get(ContextProperty);
                        viewModelNode.Content = new NetworkProxyViewModelContent(viewModelContext, viewModelContentType);
                    }

                    var networkModel = (NetworkProxyViewModelContent)viewModelNode.Content;
                    networkModel.UpdateNetworkLoadingState(stream.Read<ViewModelContentState>());
                    var flags = stream.Read<ViewModelContentFlags>();
                    if (viewModelNode.Content.LoadState == ViewModelContentState.Loaded)
                    {
                        bool updatedValue = stream.ReadBoolean();
                        if (updatedValue)
                        {
                            object value = null;
                            stream.SerializeExtended(ref value, mode, null);
                            if (networkModel.Type == typeof(ExecuteCommand))
                            {
                                value = (ExecuteCommand)((viewModel2, parameter) => networkModel.QueueCommand(parameter));
                            }

                            var lastAckPacketIndex = stream.Context.Get(LastAckPacketIndex);
                            networkModel.UpdateNetworkValue(lastAckPacketIndex, value, flags);
                        }
                    }
                }

                SerializeChildren(viewModelNode, mode, newItem, stream);
            }
        }

        public class ViewModelProxyNodeSerializer : DataSerializer<ViewModelProxyNode>
        {
            public override void Serialize(ref ViewModelProxyNode viewModelNode, ArchiveMode mode, SerializationStream stream)
            {
                var newItem = stream.Context.Get(NewItemProperty);

                if (mode == ArchiveMode.Serialize)
                {
                    if (newItem)
                    {
                        stream.Write(viewModelNode.Name != null);
                        if (viewModelNode.Name != null)
                            stream.Write(viewModelNode.Name);
                    }

                    stream.Write(viewModelNode.TargetNode.Guid);

                }
                else
                {
                    if (newItem)
                    {
                        bool hasPropertyName = stream.ReadBoolean();
                        if (hasPropertyName)
                            viewModelNode.Name = stream.ReadString();

                        var viewModelContentTypeName = stream.ReadString();

                        var viewModelContentType = Type.GetType(viewModelContentTypeName);
                        var viewModelContext = stream.Context.Get(ContextProperty);
                        viewModelNode.Content = new NetworkProxyViewModelContent(viewModelContext, viewModelContentType);
                    }

                    var guid = stream.Read<Guid>();

                    // Target GUID has changed, we need to refresh the target node. This will be done afterwards
                    if (guid != viewModelNode.TargetGuid)
                    {
                        viewModelNode.TargetNode = null;
                        viewModelNode.TargetGuid = guid;
                    }
                }
                SerializeChildren(viewModelNode, mode, newItem, stream);
            }
        }

        internal static void ProcessReference(ViewModelContext context, ViewModelReference reference, bool visible, Dictionary<IViewModelNode, bool> result)
        {
            reference.UpdateGuid(context);
            if (reference.AdditionalReferences != null)
            {
                foreach (var additionalReference in reference.AdditionalReferences)
                {
                    ProcessReference(context, additionalReference, false, result);
                }
            }
            if (reference.ViewModel != null && reference.Recursive)
                result[reference.ViewModel] = visible && reference.Visible;
        }

        internal static IEnumerable<KeyValuePair<IViewModelNode, bool>> EnumerateViewModelReferences(ViewModelContext context, IViewModelNode modelNode, bool visible)
        {
            var result = new Dictionary<IViewModelNode, bool>();

            if (modelNode.Content.Type == typeof(ViewModelReference))
            {
                var reference = ((ViewModelReference)modelNode.Content.Value);
                ProcessReference(context, reference, visible, result);
            }
            if (modelNode.Content.Type == typeof(IList<ViewModelReference>))
            {
                foreach (var reference in (IList<ViewModelReference>)modelNode.Content.Value)
                {
                    ProcessReference(context, reference, visible, result);
                }
            }

            foreach (var child in modelNode.Children)
            {
                foreach (var viewModel in EnumerateViewModelReferences(context, child, visible))
                {
                    result[viewModel.Key] = viewModel.Value;
                }
            }

            return result;
        }

        public static void UpdateReferences(ViewModelContext context, bool removeUnreferenced = false)
        {
            if (context.Root == null)
                return;

            UpdateReferences(context, context.Root, removeUnreferenced);
        }

        public static void UpdateReferences(ViewModelContext context, IViewModelNode rootNode, bool removeUnreferenced = false)
        {
            // Tells which nodes should be kept (present in dictionary) and which nodes are "visible" for export/serialization (boolean value)
            var result = new Dictionary<IViewModelNode, bool> { { rootNode, true } };

            var toProcess = new Queue<KeyValuePair<IViewModelNode, bool>>(EnumerateViewModelReferences(context, rootNode, true));

            while (toProcess.Count > 0)
            {
                var viewModel = toProcess.Dequeue();
                if (!result.ContainsKey(viewModel.Key))
                {
                    result.Add(viewModel.Key, viewModel.Value);
                    foreach (var viewModel2 in EnumerateViewModelReferences(context, viewModel.Key, viewModel.Value))
                    {
                        toProcess.Enqueue(viewModel2);
                    }
                }
            }

            if (removeUnreferenced)
            {
                context.VisibleViewModels.Clear();
                foreach (var viewModel in context.ViewModelByGuid.ToArray())
                {
                    if (!result.ContainsKey(viewModel.Value))
                    {
                        context.ViewModelByGuid.Remove(viewModel);
                    }
                    context.VisibleViewModels.Add(viewModel.Value);
                }
            }
        }

        public static byte[] NetworkSerialize(ViewModelContext context, ViewModelState state)
        {
            var memoryStream = new MemoryStream();
            var writer = new BinarySerializationWriter(memoryStream) { Context = { SerializerSelector = NetworkSerializerSelector } };
            writer.Context.Set(ContextProperty, context);
            //writer.Context.ReuseReferences = false;
            writer.Write(context.Root != null ? context.Root.Guid : Guid.Empty);
            writer.Write(context.ViewModelByGuid.Count());
            context.ContextLocked = true;
            foreach (var viewModel in context.ViewModelByGuid)
            {
                IViewModelNode oldViewModel;
                bool newVersion = !state.ViewModelByGuid.TryGetValue(viewModel.Key, out oldViewModel) || oldViewModel != viewModel.Value;
                writer.Context.Set(RootNodeProperty, viewModel.Value);
                writer.Context.Set(ContextProperty, context);
                writer.Context.Set(NewItemProperty, newVersion);
                writer.Write(newVersion);
                bool isProxy = viewModel.Value is ViewModelProxyNode;
                writer.Write(isProxy);
                //if (isProxy)
                //{
                //    writer.Write(proxy.Guid);
                //    writer.Write(proxy.Name);
                //    writer.Write(proxy.TargetNode.Guid);
                //}
                //else
                {
                    writer.Write(viewModel.Value.Guid);
                    SerializeViewModelNode(viewModel.Value, ArchiveMode.Serialize, writer);
                }
            }
            context.ContextLocked = false;
            state.ViewModelByGuid = new Dictionary<Guid, IViewModelNode>(context.ViewModelByGuid);

            return memoryStream.ToArray();
        }

        public static void NetworkDeserialize(int lastAckPacketIndex, ViewModelContext context, byte[] networkStream)
        {
            var reader = new BinarySerializationReader(new MemoryStream(networkStream)) { Context = { SerializerSelector = NetworkSerializerSelector } };
            reader.Context.Set(ContextProperty, context);
            reader.Context.Set(LastAckPacketIndex, lastAckPacketIndex);
            //reader.Context.ReuseReferences = false;
            var rootGuid = reader.Read<Guid>();
            var entityCount = reader.ReadInt32();
            var currentGuids = new HashSet<Guid>();
            for (int i = 0; i < entityCount; ++i)
            {
                bool newVersion = reader.ReadBoolean();
                bool isProxy = reader.ReadBoolean();
                reader.Context.Set(NewItemProperty, newVersion);
                var guid = reader.Read<Guid>();
                currentGuids.Add(guid);
                IViewModelNode viewModel;
                lock (context)
                {
                    viewModel = context.GetViewModelNode(guid);
                    if (viewModel == null || newVersion)
                    {
                        viewModel = isProxy ? new ViewModelProxyNode("<Serializing>", null) : new ViewModelNode("<Serializing>", new NullViewModelContent());
                        viewModel.Guid = guid;
                        context.RegisterViewModel(viewModel);
                    }
                }
                reader.Context.Set(RootNodeProperty, viewModel);
                SerializeViewModelNode(viewModel, ArchiveMode.Deserialize, reader);
            }

            lock (context)
            {
                foreach (var guid in currentGuids)
                {
                    PushNetworkValueRecursive(context.ViewModelByGuid[guid]);
                }

                context.CurrentGuids = currentGuids;

                // Remove unused viewmodels from source
                foreach (var viewModel in context.ViewModelByGuid.ToArray())
                {
                    if (!context.CurrentGuids.Contains(viewModel.Key))
                        context.ViewModelByGuid.Remove(viewModel.Key);
                }
                
                IViewModelNode root;
                context.ViewModelByGuid.TryGetValue(rootGuid, out root);
                context.Root = root;
            }
        }

        private static void BuildPath(IViewModelNode networkModelNode, List<Guid> result)
        {
            if (networkModelNode.Parent != null)
                BuildPath(networkModelNode.Parent, result);
            result.Add(networkModelNode.Guid);
        }

        public static Guid[] BuildPath(IViewModelNode networkModelNode)
        {
            var result = new List<Guid>();
            BuildPath(networkModelNode, result);
            return result.ToArray();
        }

        static void PushNetworkValueRecursive(IViewModelNode networkModelNode)
        {
            var networkContent = networkModelNode.Content as NetworkProxyViewModelContent;
            if (networkContent != null)
                networkContent.PushNetworkValue();

            foreach (var child in networkModelNode.Children)
                PushNetworkValueRecursive(child);
        }

        /*static void NetworkBuildChanges(int packetIndex, IViewModelNode networkProxyViewModelNode, List<NetworkChange> networkChanges)
        {
            // Check updated values
            var pendingValues = ((NetworkProxyViewModelContent)networkProxyViewModelNode.Content).GetPendingChanges(packetIndex);
            if (pendingValues != null)
            {
                var path = new List<Guid>();
                BuildPath(networkProxyViewModelNode, path);
                
                // Should we only send last update value?
                foreach (var pendingValue in pendingValues)
                {
                    var networkChange = pendingValue;
                    networkChange.Path = path.ToArray();
                    networkChanges.Add(networkChange);
                }
            }

            foreach (IViewModelNode child in networkProxyViewModelNode.Children)
            {
                NetworkBuildChanges(packetIndex, child, networkChanges);
            }
        }

        public static List<NetworkChange> NetworkBuildChanges(int packetIndex, ViewModelContext context)
        {
            lock (context)
            {
                var result = new List<NetworkChange>();
                foreach (var viewModel in context.ViewModelByGuid)
                {
                    NetworkBuildChanges(packetIndex, viewModel.Value, result);
                }
                return result;
            }
        }*/

        public static void NetworkApplyChanges(ViewModelContext context, NetworkChange[] changes)
        {
            foreach (var change in changes)
            {
                NetworkApplyChanges(context, change);
            }
        }

        static void NetworkApplyChanges(ViewModelContext context, NetworkChange change)
        {
            // Find property (exit nicely if not available anymore, probably due to network round-trip time -- maybe we should output a log/console warning?)
            IViewModelNode currentViewModel;
            if (!context.ViewModelByGuid.TryGetValue(change.Path[0], out currentViewModel))
                return;

            foreach (var pathElement in change.Path.Skip(1))
            {
                currentViewModel = currentViewModel.Children.FirstOrDefault(x => x.Guid == pathElement);
                if (currentViewModel == null)
                    return;
            }

            switch (change.Type)
            {
                case NetworkChangeType.ValueUpdateAsync:
                    var reader = new BinarySerializationReader(new MemoryStream((byte[])change.Value));
                    object value = null;
                    reader.SerializeExtended(ref value, ArchiveMode.Deserialize, null);
                    ((NetworkProxyViewModelContent)currentViewModel.Content).UpdateNetworkValue(int.MinValue, value, ViewModelContentFlags.None);
                    break;
                case NetworkChangeType.ValueUpdate:
                    currentViewModel.Content.Value = change.Value;
                    break;
                case NetworkChangeType.ValueRequestLoad:
                    var asyncContent = currentViewModel.Content as IAsyncViewModelContent;
                    if (asyncContent != null)
                        asyncContent.RequestLoadContent();
                    break;
                case NetworkChangeType.ActionInvoked:
                    if (currentViewModel.Content.Value != null)
                        ((ExecuteCommand)currentViewModel.Content.Value)(currentViewModel, change.Value);
                    break;
            }
        }
    }

    public class ViewModelState
    {
        public ViewModelState()
        {
            ViewModelByGuid = new Dictionary<Guid, IViewModelNode>();
        }

        public Dictionary<Guid, IViewModelNode> ViewModelByGuid { get; set; }
    }

    public enum NetworkChangeType
    {
        ValueRequestLoad,
        ValueUpdate,
        ActionInvoked,
        ValueUpdateAsync,
    }

    [DataContract]
    public struct NetworkChange
    {
        public NetworkChangeType Type;
        public Guid[] Path;
        public object Value;
    }
}