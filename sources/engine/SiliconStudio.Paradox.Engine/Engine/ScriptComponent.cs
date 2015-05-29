using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Script component.
    /// </summary>
    [DataContract("ScriptComponent")]
    [Display(10, "Scripts")]
    public sealed class ScriptComponent : EntityComponent
    {
        public static PropertyKey<ScriptComponent> Key = new PropertyKey<ScriptComponent>("Key", typeof(ScriptComponent));

        public ScriptComponent()
        {
            Scripts = new ScriptCollection();
            Scripts.CollectionChanged += (sender, args) =>
            {
                var script = (Script)args.Item;
                if (script == null)
                    return;

                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        script.ScriptComponent = this;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        script.ScriptComponent = null;
                        break;
                }
            };
        }

        /// <summary>
        /// Gets the scripts.
        /// </summary>
        /// <value>
        /// The scripts.
        /// </value>
        [Display("Script", AlwaysExpand = true)]
        public TrackingCollection<Script> Scripts { get; private set; }

        /// <inheritdoc/>
        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}