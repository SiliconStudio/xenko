// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Serializers;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Assets.Debugging
{
    /// <summary>
    /// Helper to reload game assemblies at runtime. It will update currently running scripts.
    /// </summary>
    public abstract class AssemblyReloader
    {
        protected ILogger log;
        protected readonly List<Entity> entities = new List<Entity>();

        protected virtual void RestoreReloadedScriptEntries(List<ReloadedScriptEntry> reloadedScripts)
        {
            foreach (var reloadedScript in reloadedScripts)
            {
                var scriptComponent = reloadedScript.Entity.Components.Get<ScriptComponent>();
                if (scriptComponent == null) // Should not happpen
                    continue;

                ReplaceScript(scriptComponent, reloadedScript);
            }
        }

        protected virtual List<ReloadedScriptEntry> CollectReloadedScriptEntries(HashSet<Assembly> loadedAssembliesSet)
        {
            var reloadedScripts = new List<ReloadedScriptEntry>();

            // Find scripts that will need reloading
            foreach (var entity in entities)
            {
                var scriptComponent = entity.Components.Get<ScriptComponent>();
                if (scriptComponent == null)
                    continue;

                for (int index = 0; index < scriptComponent.Scripts.Count; index++)
                {
                    var script = scriptComponent.Scripts[index];
                    if (script == null)
                        continue;

                    var scriptType = script.GetType();

                    // We force both scripts that were just unloaded and UnloadableScript (from previous failure) to try to reload
                    if (!loadedAssembliesSet.Contains(scriptType.Assembly) && scriptType != typeof(UnloadableScript))
                        continue;

                    var parsingEvents = SerializeScript(script);

                    // TODO: Serialize Scene script too (async?) -- doesn't seem necessary even for complex cases
                    // (i.e. referencing assets, entities and/or scripts) but still a ref counting check might be good

                    reloadedScripts.Add(CreateReloadedScriptEntry(entity, index, parsingEvents, script));
                }
            }
            return reloadedScripts;
        }

        protected virtual Script DeserializeScript(ReloadedScriptEntry reloadedScript)
        {
            var eventReader = new EventReader(new MemoryParser(reloadedScript.YamlEvents));
            var scriptCollection = (ScriptCollection)YamlSerializer.Deserialize(eventReader, null, typeof(ScriptCollection), log != null ? new SerializerContextSettings { Logger = new YamlForwardLogger(log) } : null);
            var script = scriptCollection.Count == 1 ? scriptCollection[0] : null;
            return script;
        }

        protected virtual List<ParsingEvent> SerializeScript(Script script)
        {
            // Wrap script in a ScriptCollection to properly handle errors
            var scriptCollection = new ScriptCollection { script };

            // Serialize with Yaml layer
            var parsingEvents = new List<ParsingEvent>();
            YamlSerializer.Serialize(new ParsingEventListEmitter(parsingEvents), scriptCollection, typeof(ScriptCollection));
            return parsingEvents;
        }

        protected virtual ReloadedScriptEntry CreateReloadedScriptEntry(Entity entity, int index, List<ParsingEvent> parsingEvents, Script script)
        {
            return new ReloadedScriptEntry(entity, index, parsingEvents);
        }

        protected virtual void ReplaceScript(ScriptComponent scriptComponent, ReloadedScriptEntry reloadedScript)
        {
            // TODO: Let PropertyGrid know that we updated the script
            scriptComponent.Scripts[reloadedScript.ScriptIndex] = DeserializeScript(reloadedScript);
        }

        protected class ReloadedScriptEntry
        {
            public readonly Entity Entity;
            public readonly int ScriptIndex;
            public readonly List<ParsingEvent> YamlEvents;

            public ReloadedScriptEntry(Entity entity, int scriptIndex, List<ParsingEvent> yamlEvents)
            {
                Entity = entity;
                ScriptIndex = scriptIndex;
                YamlEvents = yamlEvents;
            }
        }
    }
}