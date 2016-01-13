// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Manage scripts
    /// </summary>
    public sealed class ScriptProcessor : EntityProcessor<ScriptComponent, ScriptProcessor.AssociatedData>
    {
        private ScriptSystem scriptSystem;

        public ScriptProcessor()
        {
            // Script processor always running before others
            Order = -100000;
        }

        /// <inheritdoc/>
        protected override AssociatedData GenerateComponentData(Entity entity, ScriptComponent component)
        {
            return new AssociatedData(component);
        }

        protected override bool IsAssociatedDataValid(Entity entity, ScriptComponent component, AssociatedData associatedData)
        {
            return component == associatedData.Component;
        }

        protected internal override void OnSystemAdd()
        {
            scriptSystem = Services.GetServiceAs<ScriptSystem>();
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentAdding(Entity entity, ScriptComponent component, AssociatedData associatedData)
        {
            // Add current list of scripts
            var scriptComponent = (ScriptComponent)associatedData.Component;
            foreach (var script in scriptComponent.Scripts)
            {
                if(script != null)
                    scriptSystem.Add(script);
            }

            // Keep tracking changes to the collection
            associatedData.ScriptsChangedDelegate = (sender, args) =>
            {
                var script = (Script)args.Item;
                if (script == null)
                    return; 
                
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        scriptSystem.Add((Script)args.Item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        scriptSystem.Remove((Script)args.Item);
                        break;
                }
            };
            scriptComponent.Scripts.CollectionChanged += associatedData.ScriptsChangedDelegate;
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentRemoved(Entity entity, ScriptComponent component, AssociatedData associatedData)
        {
            var scriptComponent = (ScriptComponent)associatedData.Component;
            scriptComponent.Scripts.CollectionChanged -= associatedData.ScriptsChangedDelegate;
            associatedData.ScriptsChangedDelegate = null;

            // Remove scripts
            foreach (var script in scriptComponent.Scripts)
            {
                scriptSystem.Remove(script);
            }
        }

        public class AssociatedData
        {
            public EventHandler<TrackingCollectionChangedEventArgs> ScriptsChangedDelegate;

            public AssociatedData(ScriptComponent component)
            {
                Component = component;
            }

            public EntityComponent Component { get; set; }
        }
    }
}