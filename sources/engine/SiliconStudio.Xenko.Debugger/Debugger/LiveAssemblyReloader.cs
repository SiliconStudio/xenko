// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Debugging;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Debugger.Target
{
    public class LiveAssemblyReloader : AssemblyReloader
    {
        private readonly AssemblyContainer assemblyContainer;
        private readonly List<Assembly> assembliesToUnregister;
        private readonly List<Assembly> assembliesToRegister;
        private readonly Game game;

        public LiveAssemblyReloader(Game game, AssemblyContainer assemblyContainer, List<Assembly> assembliesToUnregister, List<Assembly> assembliesToRegister)
        {
            if (game != null)
                this.entities.AddRange(game.SceneSystem.SceneInstance);
            this.game = game;
            this.assemblyContainer = assemblyContainer;
            this.assembliesToUnregister = assembliesToUnregister;
            this.assembliesToRegister = assembliesToRegister;
        }

        public void Reload()
        {
            CloneReferenceSerializer.References = new List<object>();

            var loadedAssembliesSet = new HashSet<Assembly>(assembliesToUnregister);
            var reloadedScripts = CollectReloadedScriptEntries(loadedAssembliesSet);

            foreach (var assembly in assembliesToUnregister)
            {
                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Unregister(assembly);

                // Unload binary serialization
                DataSerializerFactory.UnregisterSerializationAssembly(assembly);

                // Unload assembly
                assemblyContainer.UnloadAssembly(assembly);
            }

            foreach (var assembly in assembliesToRegister)
            {
                ModuleRuntimeHelpers.RunModuleConstructor(assembly.ManifestModule);

                // Unregisters assemblies that have been registered in Package.Load => Package.LoadAssemblyReferencesForPackage
                AssemblyRegistry.Register(assembly, AssemblyCommonCategories.Assets);

                DataSerializerFactory.RegisterSerializationAssembly(assembly);
            }

            // First pass of deserialization: recreate the scripts
            foreach (ReloadedScriptEntryLive reloadedScript in reloadedScripts)
            {
                // Try to create object
                var objectStart = reloadedScript.YamlEvents.OfType<SharpYaml.Events.MappingStart>().FirstOrDefault();
                if (objectStart != null)
                {
                    // Get type info
                    var objectStartTag = objectStart.Tag;
                    bool alias;
                    var scriptType = YamlSerializer.GetSerializerSettings().TagTypeRegistry.TypeFromTag(objectStartTag, out alias);
                    if (scriptType != null)
                    {
                        reloadedScript.NewScript = (ScriptComponent)Activator.CreateInstance(scriptType);
                    }
                }
            }

            // Second pass: update script references in live objects
            // As a result, any script references processed by Yaml serializer will point to updated objects (script reference cycle will work!)
            for (int index = 0; index < CloneReferenceSerializer.References.Count; index++)
            {
                var script = CloneReferenceSerializer.References[index] as ScriptComponent;
                if (script != null)
                {
                    var reloadedScript = reloadedScripts.Cast<ReloadedScriptEntryLive>().FirstOrDefault(x => x.OriginalScript == script);
                    if (reloadedScript != null)
                    {
                        CloneReferenceSerializer.References[index] = reloadedScript.NewScript;
                    }
                }
            }

            // Third pass: deserialize
            RestoreReloadedScriptEntries(reloadedScripts);

            CloneReferenceSerializer.References = null;
        }

        protected override ReloadedScriptEntry CreateReloadedScriptEntry(Entity entity, int index, List<ParsingEvent> parsingEvents, ScriptComponent script)
        {
            return new ReloadedScriptEntryLive(entity, index, parsingEvents, script);
        }

        protected override ScriptComponent DeserializeScript(ReloadedScriptEntry reloadedScript)
        {
            var eventReader = new EventReader(new MemoryParser(reloadedScript.YamlEvents));
            var scriptCollection = new ScriptCollection();

            // Use the newly created script during second pass for proper cycle deserialization
            var newScript = ((ReloadedScriptEntryLive)reloadedScript).NewScript;
            if (newScript != null)
                scriptCollection.Add(newScript);

            // Try to create script first
            YamlSerializer.Deserialize(eventReader, scriptCollection, typeof(ScriptCollection));
            var script = scriptCollection.Count == 1 ? scriptCollection[0] : null;
            return script;
        }

        protected override List<ParsingEvent> SerializeScript(ScriptComponent script)
        {
            // Wrap script in a ScriptCollection to properly handle errors
            var scriptCollection = new ScriptCollection { script };

            // Serialize with Yaml layer
            var parsingEvents = new List<ParsingEvent>();
            // We also want to serialize live scripting variables
            var serializerContextSettings = new SerializerContextSettings { MemberMask = DataMemberAttribute.DefaultMask | ScriptComponent.LiveScriptingMask };
            YamlSerializer.Serialize(new ParsingEventListEmitter(parsingEvents), scriptCollection, typeof(ScriptCollection), serializerContextSettings);
            return parsingEvents;
        }

        protected override void ReplaceScript(ScriptComponent scriptComponent, ReloadedScriptEntry reloadedScript)
        {
            // Create new script instance
            var newScript = DeserializeScript(reloadedScript);

            // Dispose and unregister old script (and their MicroThread, if any)
            var oldScript = (ScriptComponent)scriptComponent.Entity.Components[reloadedScript.ScriptIndex];

            // Flag scripts as being live reloaded
            if (game != null)
            {
                game.Script.LiveReload(oldScript, newScript);
            }

            // Replace with new script
            // TODO: Remove script before serializing it, so cancellation code can run
            scriptComponent.Entity.Components[reloadedScript.ScriptIndex] = newScript;

            // TODO: Dispose on script?
            // oldScript.Dispose();
        }

        protected class ReloadedScriptEntryLive : ReloadedScriptEntry
        {
            // Original scripts
            public readonly ScriptComponent OriginalScript;
            public ScriptComponent NewScript;

            public ReloadedScriptEntryLive(Entity entity, int scriptIndex, List<ParsingEvent> yamlEvents, ScriptComponent originalScript)
                : base(entity, scriptIndex, yamlEvents)
            {
                OriginalScript = originalScript;
            }
        }
    }
}