// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Engine.Processors
{
    /// <summary>
    /// Manage scripts
    /// </summary>
    public sealed class ScriptProcessor : EntityProcessor<ScriptComponent>
    {
        private ScriptSystem scriptSystem;

        public ScriptProcessor()
        {
            // Script processor always running before others
            Order = -100000;
        }

        protected internal override void OnSystemAdd()
        {
            scriptSystem = Services.GetService<ScriptSystem>();
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentAdding(Entity entity, ScriptComponent component, ScriptComponent associatedData)
        {
            // Add current list of scripts
            scriptSystem.Add(component);
        }

        /// <inheritdoc/>
        protected override void OnEntityComponentRemoved(Entity entity, ScriptComponent component, ScriptComponent associatedData)
        {
            scriptSystem.Remove(component);
        }
    }
}
