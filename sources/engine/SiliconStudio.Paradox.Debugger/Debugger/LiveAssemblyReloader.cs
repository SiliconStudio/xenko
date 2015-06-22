// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Debugging;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Debugger.Target
{
    public class LiveAssemblyReloader : AssemblyReloader
    {
        private readonly AssemblyContainer assemblyContainer;
        private readonly List<Assembly> assembliesToUnregister;
        private readonly List<Assembly> assembliesToRegister;

        public LiveAssemblyReloader(Game game, AssemblyContainer assemblyContainer, List<Assembly> assembliesToUnregister, List<Assembly> assembliesToRegister)
        {
            if (game != null)
                this.entities.AddRange(game.SceneSystem.SceneInstance);
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

            RestoreReloadedScriptEntries(reloadedScripts);

            CloneReferenceSerializer.References = null;
        }

        protected override void ReplaceScript(ScriptComponent scriptComponent, ReloadedScriptEntry reloadedScript)
        {
            // Create new script instance
            var newScript = DeserializeScript(reloadedScript);

            // Dispose and unregister old script (and their MicroThread, if any)
            var oldScript = scriptComponent.Scripts[reloadedScript.ScriptIndex];
            oldScript.Dispose();

            if (oldScript.MicroThread != null && !oldScript.MicroThread.IsOver)
            {
                // Force the script to be cancelled
                oldScript.MicroThread.RaiseException(new Exception("Cancelled"));
            }

            // Replace with new script
            scriptComponent.Scripts[reloadedScript.ScriptIndex] = newScript;
        }
    }
}