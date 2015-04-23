// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Engine.Processors
{
    /// <summary>
    /// The script system handles scripts scheduling in a game.
    /// </summary>
    public sealed class ScriptSystem : GameSystemBase
    {
        /// <summary>
        /// Contains all currently executed scripts
        /// </summary>
        private HashSet<Script> registeredScripts = new HashSet<Script>();
        private HashSet<Script> scriptsToStart = new HashSet<Script>();
        private HashSet<SyncScript> syncScripts = new HashSet<SyncScript>();
        private List<Script> scriptsToStartCopy = new List<Script>();
        private List<SyncScript> syncScriptsCopy = new List<SyncScript>();

        /// <summary>
        /// Gets the scheduler.
        /// </summary>
        /// <value>The scheduler.</value>
        public Scheduler Scheduler { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSystemBase" /> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <remarks>The GameSystem is expecting the following services to be registered: <see cref="IGame" /> and <see cref="AssetManager" />.</remarks>
        public ScriptSystem(IServiceRegistry registry)
            : base(registry)
        {
            Enabled = true;
            Scheduler = new Scheduler();
            Services.AddService(typeof(ScriptSystem), this);
        }

        public override void Update(GameTime gameTime)
        {
            scriptsToStartCopy.Clear();
            scriptsToStartCopy.AddRange(scriptsToStart);

            // Start new scripts
            foreach (var script in scriptsToStartCopy)
            {
                // Start the script
                script.Start();

                // Start a microthread with execute method if it's an async script
                var asyncScript = script as AsyncScript;
                if (asyncScript != null)
                {
                    script.MicroThread = AddTask(asyncScript.Execute);
                }
            }
            scriptsToStart.Clear();

            // Run current micro threads
            Scheduler.Run();

            syncScriptsCopy.Clear();
            syncScriptsCopy.AddRange(syncScripts);

            // Execute sync scripts
            foreach (var script in syncScriptsCopy)
            {
                script.Update();
            }
        }

        /// <summary>
        /// Allows to wait for next frame.
        /// </summary>
        /// <returns>ChannelMicroThreadAwaiter&lt;System.Int32&gt;.</returns>
        public ChannelMicroThreadAwaiter<int> NextFrame()
        {
            return Scheduler.NextFrame();
        }

        /// <summary>
        /// Adds the specified micro thread function.
        /// </summary>
        /// <param name="microThreadFunction">The micro thread function.</param>
        /// <returns>MicroThread.</returns>
        public MicroThread AddTask(Func<Task> microThreadFunction)
        {
            return Scheduler.Add(microThreadFunction);
        }

        /// <summary>
        /// Waits all micro thread finished their task completion.
        /// </summary>
        /// <param name="microThreads">The micro threads.</param>
        /// <returns>Task.</returns>
        public async Task WhenAll(params MicroThread[] microThreads)
        {
            await Scheduler.WhenAll(microThreads);
        }

        /// <summary>
        /// Add the provided script to the script system.
        /// </summary>
        /// <param name="script">The script to add</param>
        public void Add(Script script)
        {
            script.Initialize(Services);
            registeredScripts.Add(script);

            // Register script for Start() and possibly async Execute()
            scriptsToStart.Add(script);

            // If it's a synchronous script, add it to the list as well
            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScripts.Add(syncScript);
            }
        }

        /// <summary>
        /// Remove the provided script from the script system.
        /// </summary>
        /// <param name="script">The script to remove</param>
        public void Remove(Script script)
        {
            // Make sure it's not registered in any pending list
            scriptsToStart.Remove(script);

            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScripts.Remove(syncScript);
            }

            registeredScripts.Remove(script);
        }

        public void ProcessAssemblyLoad(Assembly assembly)
        {
            // Check unloaded scripts if anything can be reloaded (note: making a copy since this collection will be modified)
            foreach (var script in registeredScripts.ToArray())
            {
                // We only care about unloaded scripts
                if (!script.Unloaded)
                    continue;

                // Check if same script type existing in newly loaded assembly
                var scriptType = assembly.GetType(script.GetType().FullName);
                if (scriptType == null)
                    continue;

                // Found a match, let's replace it with an instance of this new type
                var newScript = (Script)Activator.CreateInstance(scriptType);

                // Transpose old properties/fields to new instance?
                // TODO: Currently only handle simple case (properties of same type), this need to be improved! (reuse an existing system?)
                foreach (var targetProperty in scriptType.GetTypeInfo().DeclaredProperties)
                {
                    if (!CheckPropertyIsTransferable(targetProperty))
                        continue;

                    // Find a matching property (same name, type, readable & writeable)
                    var sourceProperty = script.GetType().GetTypeInfo().GetDeclaredProperty(targetProperty.Name);
                    if (sourceProperty == null || !CheckPropertyIsTransferable(sourceProperty) || sourceProperty.PropertyType != targetProperty.PropertyType)
                        continue;

                    // Copy value
                    targetProperty.SetValue(newScript, sourceProperty.GetValue(script));
                }

                // Register script in script component, or rerun it manually
                if (script.ScriptComponent != null)
                {
                    var oldScriptIndex = script.ScriptComponent.Scripts.IndexOf(script);
                    if (oldScriptIndex != -1)
                    {
                        script.ScriptComponent.Scripts[oldScriptIndex] = newScript;
                    }
                }
                else
                {
                    Add(script);
                }
            }
        }

        private static bool CheckPropertyIsTransferable(PropertyInfo targetProperty)
        {
            // Check that there is a getter and setter
            if (!targetProperty.CanRead || !targetProperty.CanWrite)
                return false;

            // Only public non-static properties
            if (!targetProperty.GetMethod.IsPublic || !targetProperty.SetMethod.IsPublic || !targetProperty.GetMethod.IsStatic)
                return false;

            // Does it contain DataMemberIgnoreAttribute?
            if (targetProperty.GetCustomAttribute<DataMemberIgnoreAttribute>() != null)
                return false;

            return true;
        }

        public void ProcessAssemblyUnload(Assembly assembly)
        {
            // Check list of running script if any should be "paused"
            foreach (var script in registeredScripts)
            {
                if (script.GetType().GetTypeInfo().Assembly != assembly)
                    continue;

                // Unregister it from launches (in case it wasn't done yet)
                scriptsToStart.Remove(script);

                // Dispose script (if it applies)
                script.Dispose();

                if (script.MicroThread != null && !script.MicroThread.IsOver)
                {
                    // Force the script to be cancelled
                    script.MicroThread.RaiseException(new Exception("Cancelled"));
                }
                else if (script.ScriptComponent == null)
                {
                    // We only care about currently running script, or script being part of a ScriptComponent (not launched manually)
                    // Script launched manually and already finished can be ignored
                    continue;
                }

                // Mark script as unloaded
                script.Unloaded = true;
            }
        }
    }
}