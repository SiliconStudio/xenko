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
    }
}