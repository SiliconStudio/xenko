// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
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
        internal readonly static Logger Log = GlobalLogger.GetLogger("ScriptSystem");

        /// <summary>
        /// Contains all currently executed scripts
        /// </summary>
        private readonly HashSet<Script> registeredScripts = new HashSet<Script>();
        private readonly HashSet<Script> scriptsToStart = new HashSet<Script>();
        private readonly HashSet<SyncScript> syncScripts = new HashSet<SyncScript>();
        private readonly List<Script> scriptsToStartCopy = new List<Script>();
        private readonly List<SyncScript> syncScriptsCopy = new List<SyncScript>();

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
            scriptsToStart.Clear();

            // Sort by priority
            scriptsToStartCopy.Sort(PriorityScriptComparer.Default);

            // Start new scripts
            foreach (var script in scriptsToStartCopy)
            {
                // Start the script
                var startupScript = script as StartupScript;
                if (startupScript != null)
                {
                    try
                    {
                        startupScript.Start();
                    }
                    catch (Exception e)
                    {
                        HandleSynchronousException(script, e);
                    }
                }

                // Start a microthread with execute method if it's an async script
                var asyncScript = script as AsyncScript;
                if (asyncScript != null)
                {
                    asyncScript.MicroThread = AddTask(asyncScript.Execute);
                    asyncScript.MicroThread.Priority = asyncScript.Priority;
                }
            }

            // Run current micro threads
            Scheduler.Run();

            // Flag scripts as not being live reloaded after starting/executing them for the first time
            foreach (var script in scriptsToStartCopy)
            {
                if (script.IsLiveReloading)
                    script.IsLiveReloading = false;
            }

            syncScriptsCopy.Clear();
            syncScriptsCopy.AddRange(syncScripts);

            // Sort by priority
            syncScriptsCopy.Sort(PriorityScriptComparer.Default);

            // Execute sync scripts
            foreach (var script in syncScriptsCopy)
            {
                try
                {
                    script.Update();
                }
                catch (Exception e)
                {
                    HandleSynchronousException(script, e);
                }
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
            var startWasPending = scriptsToStart.Remove(script);
            var wasRegistered = registeredScripts.Remove(script);

            if (!startWasPending && wasRegistered)
            {
                // Cancel scripts that were already started
                var startupScript = script as StartupScript;
                if (startupScript != null)
                {
                    try
                    {
                        startupScript.Cancel();
                    }
                    catch (Exception e)
                    {
                        HandleSynchronousException(script, e);
                    }
                }

                // TODO: Cancel async script execution
            }

            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScripts.Remove(syncScript);
            }
        }

        /// <summary>
        /// Called by a live scripting debugger to notify the ScriptSystem about reloaded scripts.
        /// </summary>
        /// <param name="oldScript">The old script</param>
        /// <param name="newScript">The new script</param>
        public void LiveReload(Script oldScript, Script newScript)
        {
            // Set live reloading mode for the rest of it's lifetime
            oldScript.IsLiveReloading = true;

            // Set live reloading mode until after being started
            newScript.IsLiveReloading = true;
        }

        private void HandleSynchronousException(Script script, Exception e)
        {
            Log.Error("Unexpected exception while executing a script. Reason: {0}", new object[] { e });

            // Only crash if live scripting debugger is not listening
            if (Scheduler.PropagateExceptions)
                ExceptionDispatchInfo.Capture(e).Throw();

            // Remove script from all lists
            var syncScript = script as SyncScript;
            if (syncScript != null)
            {
                syncScripts.Remove(syncScript);
            }

            registeredScripts.Remove(script);
        }

        class PriorityScriptComparer : IComparer<Script>
        {
            public static readonly PriorityScriptComparer Default = new PriorityScriptComparer();

            public int Compare(Script x, Script y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }
    }
}