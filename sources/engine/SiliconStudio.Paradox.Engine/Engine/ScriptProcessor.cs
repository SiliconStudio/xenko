using System;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Manage scripts
    /// </summary>
    public sealed class ScriptProcessor : EntityProcessor<ScriptProcessor.AssociatedData>
    {
        private ScriptSystem scriptSystem;

        public ScriptProcessor() : base(new PropertyKey[] { ScriptComponent.Key })
        {
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
                    scriptSystem.AddScript(script);
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
                        scriptSystem.AddScript((Script)args.Item);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        scriptSystem.RemoveScript((Script)args.Item);
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
                scriptSystem.RemoveScript(script);
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