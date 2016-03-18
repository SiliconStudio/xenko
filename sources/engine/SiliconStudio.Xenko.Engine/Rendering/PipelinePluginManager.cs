using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Manages <see cref="IPipelinePlugin"/>.
    /// </summary>
    public class PipelinePluginManager
    {
        private static readonly List<AutomaticPipelinePlugin> automaticPlugins = new List<AutomaticPipelinePlugin>();
        private readonly Dictionary<Type, PipelinePluginInstantiation> pipelinePlugins = new Dictionary<Type, PipelinePluginInstantiation>();
        private readonly RenderSystem renderSystem;

        public PipelinePluginManager(RenderSystem renderSystem)
        {
            this.renderSystem = renderSystem;
        }

        public static void RegisterAutomaticPlugin(Type pluginType, params Type[] dependentTypes)
        {
            lock (automaticPlugins)
            {
                automaticPlugins.Add(new AutomaticPipelinePlugin(pluginType, dependentTypes));
            }
        }

        public T GetPlugin<T>() where T : IPipelinePlugin
        {
            return (T)GetPlugin(typeof(T), false);
        }

        public IPipelinePlugin InstantiatePlugin(Type pipelinePluginType)
        {
            return GetPlugin(pipelinePluginType, true);
        }

        public T InstantiatePlugin<T>() where T : IPipelinePlugin
        {
            return (T)InstantiatePlugin(typeof(T));
        }

        public void ReleasePlugin<T>() where T : IPipelinePlugin
        {
            ReleasePlugin(typeof(T));
        }

        private void ReleasePlugin(Type pipelinePluginType)
        {
            lock (pipelinePlugins)
            {
                PipelinePluginInstantiation pipelinePlugin;
                int newCounter;
                if (!pipelinePlugins.TryGetValue(pipelinePluginType, out pipelinePlugin) || (newCounter = --pipelinePlugin.Counter) < 0)
                {
                    throw new InvalidOperationException("Cannot release plugin that don't have active references");
                }

                if (newCounter == 0)
                {
                    CheckAutomaticPlugins(pipelinePluginType, false);
                    pipelinePlugin.Instance.Unload(new PipelinePluginContext(renderSystem.RenderContextOld, renderSystem));
                }
            }
        }

        private bool IsPluginInstantiated(Type pipelinePluginType)
        {
            lock (pipelinePlugins)
            {
                PipelinePluginInstantiation pipelinePlugin;
                if (pipelinePlugins.TryGetValue(pipelinePluginType, out pipelinePlugin))
                {
                    return pipelinePlugin.Counter > 0;
                }

                return false;
            }
        }

        private IPipelinePlugin GetPlugin(Type pipelinePluginType, bool incrementCount)
        {
            lock (pipelinePlugins)
            {
                PipelinePluginInstantiation pipelinePlugin;
                if (!pipelinePlugins.TryGetValue(pipelinePluginType, out pipelinePlugin))
                {
                    pipelinePlugin = new PipelinePluginInstantiation((IPipelinePlugin)Activator.CreateInstance(pipelinePluginType));
                    pipelinePlugins.Add(pipelinePluginType, pipelinePlugin);
                }

                if (incrementCount)
                {
                    if (++pipelinePlugin.Counter == 1)
                    {
                        pipelinePlugin.Instance.Load(new PipelinePluginContext(renderSystem.RenderContextOld, renderSystem));
                        CheckAutomaticPlugins(pipelinePluginType, true);
                    }
                }

                return pipelinePlugin.Instance;
            }
        }

        // isTriggerTypeAlive represents the state of the plugin being added (true) or removed(false)
        private void CheckAutomaticPlugins(Type triggerType, bool isTriggerTypeAlive)
        {
            // Check if this type might be affected
            // TODO: We could optimize this by preregistering which type affect which types
            lock (automaticPlugins)
            {
                foreach (var automaticPlugin in automaticPlugins)
                {
                    if (Array.IndexOf(automaticPlugin.DependentTypes, triggerType) != -1)
                    {
                        // Found a type, let's check if its state changed
                        // First, let's check if is is already instantiated (in case of adding) or removed (in case of removing)
                        if (IsPluginInstantiated(automaticPlugin.Type) == isTriggerTypeAlive)
                        {
                            // No need to check further
                            continue;
                        }

                        // Check if it needs to be instantiated or removed (do all dependencies match?)
                        bool dependenciesMatch = true;
                        foreach (var dependentType in automaticPlugin.DependentTypes)
                        {
                            if (!IsPluginInstantiated(dependentType))
                            {
                                dependenciesMatch = false;
                                break;
                            }
                        }

                        if (dependenciesMatch == isTriggerTypeAlive)
                        {
                            // Need to instantiate or delete this plugin
                            if (isTriggerTypeAlive)
                                GetPlugin(automaticPlugin.Type, true);
                            else
                                ReleasePlugin(automaticPlugin.Type);
                        }
                    }
                }
            }
        }

        class PipelinePluginInstantiation
        {
            public readonly IPipelinePlugin Instance;
            public int Counter;

            public PipelinePluginInstantiation(IPipelinePlugin instance)
            {
                Instance = instance;
            }
        }

        struct AutomaticPipelinePlugin
        {
            public readonly Type Type;
            public readonly Type[] DependentTypes;

            public AutomaticPipelinePlugin(Type type, Type[] dependentTypes)
            {
                Type = type;
                DependentTypes = dependentTypes;
            }
        }
    }
}