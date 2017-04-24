// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SiliconStudio.Xenko;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.EntityModel;
using SiliconStudio.Xenko.Configuration;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Games.IO;
using SiliconStudio.Xenko.Games.MicroThreading;
using SiliconStudio.Xenko.Games.ViewModel;
using SiliconStudio.Xenko.Games.Serialization;
using SiliconStudio.Xenko.Prefabs;

using ScriptTest2;
#if NET45
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace ScriptTest
{
    [Framework.Serialization.SerializableExtended]
    public class EntitiesUpdatePacket
    {
        /// <summary>
        /// Gets or sets the index of the last acknowledged packet.
        /// This is useful for client-side prediction, i.e. when user input values
        /// and they should still be displayed until at least one packet round-trip
        /// has been done. After that time, engine value coming from new update
        /// packets should be up to date.
        /// </summary>
        public int AckIndex { get; set; }
        public Dictionary<string, byte[]> Data { get; set; }
    }

    [Framework.Serialization.SerializableExtended]
    public class EntitiesChangePacket
    {
        /// <summary>
        /// Gets or sets the index of this update packet.
        /// </summary>
        public int Index { get; set; }
        public string GroupKey { get; set; }
        public NetworkChange[] Changes { get; set; }
    }
    
    [XenkoScript]
    public class ScriptDebug
    {
        private static ViewModelContext selectedEntitiesContext;
        private static AsyncSignal entitiesChangePacketEvent = new AsyncSignal();
        private static PickingSystem pickingSystem;

        public class Config
        {
            public Config()
            {
                DebugManager = false;
                Port = 11000;
            }

            [XmlAttribute("debugmanager")]
            public bool DebugManager { get; set; }

            [XmlAttribute("port")]
            public int Port { get; set; }
        }

        public class PendingClient
        {
            public SocketContext MainSocket;
            public SocketContext AsyncSocket;
        }

        public static bool IsDebugManager
        {
            get
            {
                var configDebug = AppConfig.GetConfiguration<Config>("ScriptDebug");
                return (configDebug.DebugManager);
            }
        }

        public static void SelectEntity(params Entity[] entities)
        {
            SelectEntity((IEnumerable<Entity>)entities);
        }

        public static void SelectEntity(IEnumerable<Entity> entities)
        {
            // Update property editor selection.
            if (selectedEntitiesContext != null)
            {
                selectedEntitiesContext.ViewModelByGuid.Clear();
                var viewModels = entities
                    .Where(entity => entity != null)
                    .Select(entity => selectedEntitiesContext.GetModelView(entity).Children.First(x => x.PropertyName == "Components"))
                    .ToArray();

                if (viewModels.Count() > 1)
                    selectedEntitiesContext.Root = ViewModelController.Combine(selectedEntitiesContext, viewModels);
                else
                    selectedEntitiesContext.Root = viewModels.FirstOrDefault();
            }

            // Update picking system (gizmo).
            // It will also update the remote selection in entity tree view.
            var entitiesArray = entities.ToArray();
            if (!ArrayExtensions.ArraysEqual(pickingSystem.SelectedEntities, entitiesArray))
                pickingSystem.SelectedEntities = entitiesArray;

            entitiesChangePacketEvent.Set();
        }

        public static async Task RunDebug(EngineContext engineContext)
        {
            var config = AppConfig.GetConfiguration<Config>("ScriptDebug");
            var renderingSetup = RenderingSetup.Singleton;

            engineContext.RenderContext.PrepareEffectPlugins += (effectBuilder, plugins) =>
                {
                    if (effectBuilder.PickingPassMainPlugin != null)
                    {
                        RenderPassPlugin pickingPlugin;
                        if (engineContext.DataContext.RenderPassPlugins.TryGetValue(effectBuilder.Name == "Gizmo" ? "MouseOverPickingPlugin" : "PickingPlugin", out pickingPlugin))
                            plugins.Add(new PickingShaderPlugin { RenderPassPlugin = (PickingPlugin)pickingPlugin, MainPlugin = effectBuilder.PickingPassMainPlugin });
                    }
                    if (effectBuilder.SupportWireframe)
                    {
                        RenderPassPlugin wireframePlugin;
                        if (engineContext.DataContext.RenderPassPlugins.TryGetValue("WireframePlugin", out wireframePlugin))
                            plugins.Add(new WireframeShaderPlugin { RenderPassPlugin = wireframePlugin, MainTargetPlugin = renderingSetup.MainTargetPlugin });
                    }
                };

            pickingSystem = new PickingSystem();
            pickingSystem.PropertyChanged += pickingSystem_PropertyChanged;
            engineContext.Scheduler.Add(() => pickingSystem.ProcessGizmoAndPicking(engineContext));

            var socketContext = new SocketContext();
            var socketContextAsync = new SocketContext();

            var currentScheduler = Scheduler.Current;

            var pendingClient = new PendingClient();

            socketContext.Connected = (clientSocketContext) =>
                {
                    lock (pendingClient)
                    {
                        pendingClient.MainSocket = clientSocketContext;
                        if (pendingClient.AsyncSocket != null)
                            currentScheduler.Add(() => ProcessClient(engineContext, pendingClient.MainSocket, pendingClient.AsyncSocket));
                    }
                };
            socketContextAsync.Connected = (clientSocketContext) =>
                {
                    lock (pendingClient)
                    {
                        pendingClient.AsyncSocket = clientSocketContext;
                        if (pendingClient.MainSocket != null)
                            currentScheduler.Add(() => ProcessClient(engineContext, pendingClient.MainSocket, pendingClient.AsyncSocket));
                    }
                };

            socketContext.StartServer(config.Port);
            socketContextAsync.StartServer(config.Port + 1);
        }

        static void pickingSystem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedEntity")
            {
                SelectEntity(pickingSystem.SelectedEntities);
            }
        }

        public async static Task ProcessClient(EngineContext engineContext, SocketContext socketContext, SocketContext socketContextAsync)
        {
            socketContext.AddPacketHandler<DownloadFileQuery>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Open, VirtualFileAccess.Read);
                    var data = new byte[stream.Length];
                    await stream.ReadAsync(data, 0, data.Length);
                    stream.Close();
                    socketContext.Send(new DownloadFileAnswer { StreamId = packet.StreamId, Data = data });
                });

            socketContext.AddPacketHandler<UploadFilePacket>(
                async (packet) =>
                {
                    var stream = await VirtualFileSystem.OpenStreamAsync(packet.Url, VirtualFileMode.Create, VirtualFileAccess.Write);
                    await stream.WriteAsync(packet.Data, 0, packet.Data.Length);
                    stream.Close();
                });

            var viewModelGlobalContext = new ViewModelGlobalContext();

            selectedEntitiesContext = new ViewModelContext(viewModelGlobalContext);
            selectedEntitiesContext.ChildrenPropertyEnumerators.Add(new EntityComponentEnumerator(engineContext));
            selectedEntitiesContext.ChildrenPropertyEnumerators.Add(new RenderPassPluginEnumerator());
            selectedEntitiesContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            //selectedEntitiesContext.ChildrenPropertyEnumerators.Add(new EffectPropertyEnumerator(engineContext));

            var renderPassHierarchyContext = new ViewModelContext(viewModelGlobalContext);
            renderPassHierarchyContext.ChildrenPropertyEnumerators.Add(new RenderPassHierarchyEnumerator());
            renderPassHierarchyContext.Root = new ViewModelNode("Root", engineContext.RenderContext.RootRenderPass).GenerateChildren(renderPassHierarchyContext);

            var renderPassPluginsContext = new ViewModelContext(viewModelGlobalContext);
            renderPassPluginsContext.ChildrenPropertyEnumerators.Add(new RenderPassPluginsEnumerator { SelectedRenderPassPluginContext = selectedEntitiesContext });
            renderPassPluginsContext.Root = new ViewModelNode("Root", new EnumerableViewModelContent<ViewModelReference>(
                () => engineContext.RenderContext.RenderPassPlugins.Select(x => new ViewModelReference(x, true))));


            var entityHierarchyEnumerator = new EntityHierarchyEnumerator(engineContext.EntityManager, selectedEntitiesContext);
            var entityHierarchyContext = new ViewModelContext(viewModelGlobalContext);
            entityHierarchyContext.ChildrenPropertyEnumerators.Add(entityHierarchyEnumerator);
            entityHierarchyContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            entityHierarchyContext.Root = new ViewModelNode("EntityHierarchyRoot", new EnumerableViewModelContent<ViewModelReference>(
                        () => engineContext.EntityManager.Entities
                                           .Where(x =>
                                           {
                                               var transformationComponent = x.Transformation;
                                               return (transformationComponent == null || transformationComponent.Parent == null);
                                           })
                                           .Select(x => new ViewModelReference(x, true))));

            entityHierarchyEnumerator.SelectedEntities.CollectionChanged += (sender, args) =>
                {
                    SelectEntity(entityHierarchyEnumerator.SelectedEntities);
                };
            //entityHierarchyContext.Root.Children.Add(new ViewModelNode("SelectedItems", EnumerableViewModelContent.FromUnaryLambda<ViewModelReference, ViewModelReference>(new NullViewModelContent(),
            //    (x) => { return new[] { new ViewModelReference(pickingSystem.SelectedEntity) }; })));
                /*(value) =>
                    {
                        var entityModelView = value != null ? entityHierarchyContext.GetModelView(value.Guid) : null;
                        var entity = entityModelView != null ? (Entity)entityModelView.NodeValue : null;
                        SelectEntity(entity);
                    })));*/
            entityHierarchyContext.Root.Children.Add(new ViewModelNode("DropEntity", new RootViewModelContent((ExecuteCommand)((viewModel2, parameter) =>
                {
                    var dropParameters = (DropCommandParameters)parameter;

                    var movedItem = dropParameters.Data is Guid ? entityHierarchyContext.GetModelView((Guid)dropParameters.Data) : null;
                    var newParent = dropParameters.Parent is Guid ? entityHierarchyContext.GetModelView((Guid)dropParameters.Parent) : null;

                    if (newParent == null || movedItem == null)
                        return;

                    var parent = ((Entity)newParent.NodeValue).Transformation;
                    if (dropParameters.TargetIndex > parent.Children.Count)
                        return;

                    var transformationComponent = ((Entity)movedItem.NodeValue).Transformation;
                    transformationComponent.Parent = null;
                    parent.Children.Insert(dropParameters.TargetIndex, transformationComponent);
                }))));

            entityHierarchyContext.Root.Children.Add(new ViewModelNode("DropAsset", new RootViewModelContent((ExecuteCommand)(async (viewModel2, parameter) =>
                {
                    var dropParameters = (DropCommandParameters)parameter;

                    var assetUrl = (string)dropParameters.Data;
                    /*var newParent = entityHierarchyContext.GetModelView((Guid)dropParameters.Parent);

                    if (newParent == null || assetUrl == null)
                        return;

                    var parent = ((Entity)newParent.NodeValue).Transformation;
                    if (dropParameters.ItemIndex > parent.Children.Count)
                        return;*/

                    engineContext.Scheduler.Add(async () =>
                    {
                        // Load prefab entity
                        var loadedEntityPrefab = await engineContext.AssetManager.LoadAsync<Entity>(assetUrl + "#");

                        // Build another entity from prefab
                        var loadedEntity = Prefab.Inherit(loadedEntityPrefab);

                        // Add it to scene
                        engineContext.EntityManager.AddEntity(loadedEntity);

                        if (loadedEntity.ContainsKey(AnimationComponent.Key))
                        {
                            Scheduler.Current.Add(() => AnimScript.AnimateFBXModel(engineContext, loadedEntity));
                        }
                    });
                }))));

            var scriptEngineContext = new ViewModelContext(viewModelGlobalContext);
            scriptEngineContext.ChildrenPropertyEnumerators.Add(new ScriptAssemblyEnumerator(engineContext));
            scriptEngineContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            scriptEngineContext.Root = new ViewModelNode(new EnumerableViewModelContent<ViewModelReference>(
                        () => engineContext.ScriptManager.ScriptAssemblies.Select(x => new ViewModelReference(x, true))));
            scriptEngineContext.Root.Children.Add(new ViewModelNode("RunScript", new RootViewModelContent((ExecuteCommand)(async (viewModel2, parameter) =>
                {
                    var scriptName = (string)parameter;
                    var matchingScript = engineContext.ScriptManager.Scripts.Where(x => x.TypeName + "." + x.MethodName == scriptName);
                    if (matchingScript.Any())
                    {
                        var scriptEntry = matchingScript.Single();
                        var microThread = engineContext.ScriptManager.RunScript(scriptEntry, null);
                    }
                }))));

            var runningScriptsContext = new ViewModelContext(viewModelGlobalContext);
            runningScriptsContext.ChildrenPropertyEnumerators.Add(new MicroThreadEnumerator(selectedEntitiesContext));
            runningScriptsContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            runningScriptsContext.Root = new ViewModelNode("MicroThreads", new EnumerableViewModelContent<ViewModelReference>(
                    () => engineContext.Scheduler.MicroThreads.Select(x => new ViewModelReference(x, true))
                ));

            var effectsContext = new ViewModelContext(viewModelGlobalContext);
            effectsContext.ChildrenPropertyEnumerators.Add(new EffectEnumerator(selectedEntitiesContext));
            effectsContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            effectsContext.Root = new ViewModelNode("Effects", new EnumerableViewModelContent<ViewModelReference>(
                    () => engineContext.RenderContext.Effects.Select(x => new ViewModelReference(x, true))
                ));
            //effectsContext.Root.Children.Add(new ViewModelNode("PluginDefinitions", new RootViewModelContent()));

            var assetBrowserContext = new ViewModelContext(viewModelGlobalContext);
            assetBrowserContext.ChildrenPropertyEnumerators.Add(new AssetBrowserEnumerator(engineContext));
            assetBrowserContext.ChildrenPropertyEnumerators.Add(new ChildrenPropertyInfoEnumerator());
            assetBrowserContext.Root = new ViewModelNode("Root", "Root").GenerateChildren(assetBrowserContext);

            var editorContext = new ViewModelContext(viewModelGlobalContext);
            editorContext.Root = new ViewModelNode("Root");
            editorContext.Root.Children.Add(new ViewModelNode("SwitchSelectionMode", new CommandViewModelContent((sender, parameters) => { pickingSystem.ActiveGizmoActionMode = PickingSystem.GizmoAction.None; })));
            editorContext.Root.Children.Add(new ViewModelNode("SwitchTranslationMode", new CommandViewModelContent((sender, parameters) => { pickingSystem.ActiveGizmoActionMode = PickingSystem.GizmoAction.Translation; })));
            editorContext.Root.Children.Add(new ViewModelNode("SwitchRotationMode", new CommandViewModelContent((sender, parameters) => { pickingSystem.ActiveGizmoActionMode = PickingSystem.GizmoAction.Rotation; })));

            var contexts = new Dictionary<string, Tuple<ViewModelContext, ViewModelState>>();
            contexts.Add("Editor", Tuple.Create(editorContext, new ViewModelState()));
            contexts.Add("RenderPassPlugins", Tuple.Create(renderPassPluginsContext, new ViewModelState()));
            contexts.Add("RenderPasses", Tuple.Create(renderPassHierarchyContext, new ViewModelState()));
            contexts.Add("SelectedEntities", Tuple.Create(selectedEntitiesContext, new ViewModelState()));
            contexts.Add("EntityHierarchy", Tuple.Create(entityHierarchyContext, new ViewModelState()));
            contexts.Add("ScriptEngine", Tuple.Create(scriptEngineContext, new ViewModelState()));
            contexts.Add("MicroThreads", Tuple.Create(runningScriptsContext, new ViewModelState()));
            contexts.Add("AssetBrowser", Tuple.Create(assetBrowserContext, new ViewModelState()));
            contexts.Add("Effects", Tuple.Create(effectsContext, new ViewModelState()));

            int lastAckPacket = 0;

            var entitiesChangePackets = new ConcurrentQueue<EntitiesChangePacket>();
            socketContext.AddPacketHandler<EntitiesChangePacket>(
                (packet) =>
                    {
                        entitiesChangePackets.Enqueue(packet);
                        entitiesChangePacketEvent.Set();
                    });

            Action asyncThreadStart = () =>
                {
                    while (true)
                    {
                        Thread.Sleep(100);
                        foreach (var context in contexts)
                        {
                            // Process async data
                            Guid[] path = null;
                            object value = null;
                            lock (context.Value.Item1)
                            {
                                var pendingNode = context.Value.Item1.GetNextPendingAsyncNode();
                                if (pendingNode != null)
                                {
                                    value = pendingNode.Value;
                                    path = ViewModelController.BuildPath(pendingNode);
                                }
                            }
                            if (path != null)
                            {
                                // Temporary encoding through our serializer (until our serializer are used for packets)
                                var memoryStream = new MemoryStream();
                                var writer = new BinarySerializationWriter(memoryStream);
                                writer.SerializeExtended(null, value, ArchiveMode.Serialize);

                                var change = new NetworkChange { Path = path.ToArray(), Type = NetworkChangeType.ValueUpdateAsync, Value = memoryStream.ToArray() };
                                var packet = new EntitiesChangePacket { GroupKey = context.Key, Changes = new NetworkChange[] { change } };
                                socketContextAsync.Send(packet);
                                break;
                            }
                        }
                    }
                };

            new Thread(new ThreadStart(asyncThreadStart)).Start();

            // TODO: Move some of this code directly inside ViewModelContext/Controller classes
            while (true)
            {
                await TaskEx.WhenAny(TaskEx.Delay(250), entitiesChangePacketEvent.WaitAsync());

                EntitiesChangePacket packet;
                while (entitiesChangePackets.TryDequeue(out packet))
                {
                    ViewModelController.NetworkApplyChanges(contexts[packet.GroupKey].Item1, packet.Changes);
                    lastAckPacket = packet.Index;
                }

                // Wait a single frame so that network updates get applied properly by all rendering systems for next update
                await Scheduler.Current.NextFrame();

                // If entity disappeared, try to replace it with new one (happen during file reload)
                // It's little bit cumbersome to test, need some simplification of this specific entity view model root.
                if (selectedEntitiesContext.Root != null
                    && selectedEntitiesContext.Root.Parent != null
                    && selectedEntitiesContext.Root.Parent.NodeValue is Entity)
                {
                    var entity = (Entity)selectedEntitiesContext.Root.Parent.NodeValue;
                    if (!engineContext.EntityManager.Entities.Contains(entity))
                    {
                        entity = engineContext.EntityManager.Entities.FirstOrDefault(x => x.Guid == entity.Guid);
                        if (entity != null)
                        {
                            selectedEntitiesContext.ViewModelByGuid.Clear();
                            selectedEntitiesContext.Root = selectedEntitiesContext.GetModelView(entity).Children.First(x => x.PropertyName == "Components");
                        }
                    }
                }

                var data = new Dictionary<string, byte[]>();
                foreach (var context in contexts)
                {
                    lock (context.Value.Item1)
                    {
                        if (context.Value.Item1.Root != null)
                            context.Value.Item1.AddModelView(context.Value.Item1.Root);
                        ViewModelController.UpdateReferences(context.Value.Item1, true);
                        data[context.Key] = ViewModelController.NetworkSerialize(context.Value.Item1, context.Value.Item2);
                    }
                }

                viewModelGlobalContext.UpdateObjects(contexts.Select(x => x.Value.Item1));

                //Console.WriteLine("DataSize: {0}", data.Sum(x => x.Value.Length));
                await Task.Factory.StartNew(() => socketContext.Send(new EntitiesUpdatePacket { AckIndex = lastAckPacket, Data = data }));
            }
        }
    }
}
