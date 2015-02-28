// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Paradox
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
        private HashSet<Script> scriptsToExecute = new HashSet<Script>();

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
            // Run current micro threads
            Scheduler.Run();

            // Execute new scripts
            foreach (var script in scriptsToExecute)
            {
                script.MicroThread = Add(script.Execute);
            }
            scriptsToExecute.Clear();
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
        public MicroThread Add(Func<Task> microThreadFunction)
        {
            return Scheduler.Add(microThreadFunction);
        }

        /// <summary>
        /// Adds the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns>MicroThread.</returns>
        public MicroThread Add(IScript script)
        {
            return Scheduler.Add(script.Execute);
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

        public void AddScript(Script script)
        {
            script.Initialize(Services);
            registeredScripts.Add(script);

            // Note: there might be new types of scripts later (Update() based, etc...)
            scriptsToExecute.Add(script);
        }

        public void RemoveScript(Script script)
        {
            // Make sure it's not registered in any pending list
            scriptsToExecute.Remove(script);

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
                foreach (var targetProperty in scriptType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!targetProperty.CanRead || !targetProperty.CanWrite)
                        continue;

                    // Find a matching property (same name, type, readable & writeable)
                    var sourceProperty = script.GetType().GetProperty(targetProperty.Name, BindingFlags.Instance | BindingFlags.Public);
                    if (sourceProperty == null || !sourceProperty.CanRead || !sourceProperty.CanWrite || sourceProperty.PropertyType != targetProperty.PropertyType)
                        continue;

                    // Does it contain DataMemberIgnoreAttribute?
                    if (targetProperty.GetCustomAttribute<DataMemberIgnoreAttribute>() != null)
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
                    AddScript(script);
                }
            }
        }

        public void ProcessAssemblyUnload(Assembly assembly)
        {
            // Check list of running script if any should be "paused"
            foreach (var script in registeredScripts)
            {
                if (script.GetType().Assembly != assembly)
                    continue;

                // Unregister it from launches (in case it wasn't done yet)
                scriptsToExecute.Remove(script);

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