// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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

        /// <inheritdoc/>
        protected override ScriptComponent GenerateComponentData(Entity entity, ScriptComponent component)
        {
            return component;
        }

        protected override bool IsAssociatedDataValid(Entity entity, ScriptComponent component, ScriptComponent associatedData)
        {
            return component == associatedData;
        }

        protected internal override void OnSystemAdd()
        {
            scriptSystem = Services.GetServiceAs<ScriptSystem>();
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