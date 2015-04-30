using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// Manage scripts
    /// </summary>
    public sealed class ScriptProcessor : EntityProcessor<ScriptProcessor.AssociatedData>
    {
        private ScriptSystem scriptSystem;

        public ScriptProcessor() : base(new PropertyKey[] { ScriptComponent.Key })
        {
            // Script processor always running before others
            Order = -100000;
        }

        /// <inheritdoc/>
        protected override AssociatedData GenerateAssociatedData(Entity entity)
        {
            return new AssociatedData(entity.Get<ScriptComponent>());
        }

        protected internal override void OnSystemAdd()
        {
            scriptSystem = Services.GetServiceAs<ScriptSystem>();
        }

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, AssociatedData associatedData)
        {
            // Add current list of scripts
            foreach (var script in associatedData.Component.Scripts)
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
            associatedData.Component.Scripts.CollectionChanged += associatedData.ScriptsChangedDelegate;
        }

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, AssociatedData associatedData)
        {
            associatedData.Component.Scripts.CollectionChanged -= associatedData.ScriptsChangedDelegate;
            associatedData.ScriptsChangedDelegate = null;

            // Remove scripts
            foreach (var script in associatedData.Component.Scripts)
            {
                scriptSystem.Remove(script);
            }
        }

        public class AssociatedData
        {
            public ScriptComponent Component;
            public EventHandler<TrackingCollectionChangedEventArgs> ScriptsChangedDelegate;

            public AssociatedData(ScriptComponent component)
            {
                Component = component;
            }
        }
    }
}