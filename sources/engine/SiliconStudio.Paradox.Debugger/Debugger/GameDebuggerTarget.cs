// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Debugger.Target
{
    public class GameDebuggerTarget : IGameDebuggerTarget
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("GameDebuggerSession");

        /// <summary>
        /// The assembly container, to load assembly without locking main files.
        /// </summary>
        // For now, it uses default one, but later we should probably have one per game debugger session
        private AssemblyContainer assemblyContainer = AssemblyContainer.Default;

        private string projectName;
        private Dictionary<DebugAssembly, Assembly> loadedAssemblies = new Dictionary<DebugAssembly, Assembly>();
        private int currentDebugAssemblyIndex;
        private Game game;

        private ManualResetEvent gameFinished = new ManualResetEvent(true);
        private IGameDebuggerHost host;

        private bool requestedExit;

        public GameDebuggerTarget()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            lock (loadedAssemblies)
            {
                return loadedAssemblies.Values.FirstOrDefault(x => x.FullName == args.Name);
            }
        }

        /// <inheritdoc/>
        public void Exit()
        {
            requestedExit = true;
        }

        /// <inheritdoc/>
        public DebugAssembly AssemblyLoad(string assemblyPath)
        {
            try
            {
                var assembly = assemblyContainer.LoadAssemblyFromPath(assemblyPath);
                if (assembly == null)
                {
                    Log.Error("Unexpected error while loading assembly reference [{0}] in project [{1}]", assemblyPath, projectName);
                    return DebugAssembly.Empty;
                }

                AssemblyOnLoad(assembly);
                return CreateDebugAssembly(assembly);
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while loading assembly reference [{0}] in project [{1}]", ex, assemblyPath, projectName);
                return DebugAssembly.Empty;
            }
        }

        /// <inheritdoc/>
        public DebugAssembly AssemblyLoadRaw(byte[] peData, byte[] pdbData)
        {
            try
            {
                lock (loadedAssemblies)
                {
                    var assembly = Assembly.Load(peData, pdbData);
                    AssemblyOnLoad(assembly);
                    return CreateDebugAssembly(assembly);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected error while loading assembly reference in project [{0}]", ex, projectName);
                return DebugAssembly.Empty;
            }
        }

        /// <inheritdoc/>
        public bool AssemblyUnload(DebugAssembly debugAssembly)
        {
            // Unload assembly in assemblyContainer
            lock (loadedAssemblies)
            {
                Assembly assembly;
                if (!loadedAssemblies.TryGetValue(debugAssembly, out assembly))
                    return false;

                assemblyContainer.UnloadAssembly(assembly);
                loadedAssemblies.Remove(debugAssembly);
                AssemblyOnUnload(assembly);
            }
            return true;
        }

        private void AssemblyOnLoad(Assembly assembly)
        {
            // Ensure module ctor has run (register serializers, etc...)
            RuntimeHelpers.RunModuleConstructor(assembly.ManifestModule.ModuleHandle);

            if (game != null)
            {
                game.Script.ProcessAssemblyLoad(assembly);
            }
        }
        private void AssemblyOnUnload(Assembly assembly)
        {
            if (game != null)
            {
                game.Script.ProcessAssemblyUnload(assembly);
            }
        }

        /// <inheritdoc/>
        public List<string> GameEnumerateTypeNames()
        {
            lock (loadedAssemblies)
            {
                return GameEnumerateTypesHelper().Select(x => x.FullName).ToList();
            }
        }

        /// <inheritdoc/>
        public void GameLaunch(string gameTypeName)
        {
            try
            {
                Type gameType;
                lock (loadedAssemblies)
                {
                    gameType = GameEnumerateTypesHelper().FirstOrDefault(x => x.FullName == gameTypeName);
                }

                if (gameType == null)
                    throw new InvalidOperationException(string.Format("Could not find type [{0}] in project [{1}]", gameTypeName, projectName));

                game = (Game)Activator.CreateInstance(gameType);

                // TODO: Bind database
                Task.Run(() =>
                {
                    gameFinished.Reset();
                    try
                    {
                        using (game)
                        {
                            game.Run();
                        }
                    }
                    catch (Exception e)
                    {
                        // Mute exceptions
                        // TODO: Transfer them back to listening process?
                    }

                    host.OnGameExited();

                    // Notify we are done
                    gameFinished.Set();
                });
            }
            catch (Exception ex)
            {
                Log.Error("Game [{0}] from project [{1}] failed to run", ex, gameTypeName, projectName);
            }
        }

        /// <inheritdoc/>
        public void GameStop()
        {
            if (game == null)
                return;

            game.Exit();

            // Wait for game to actually exit?
            gameFinished.WaitOne();

            game = null;
        }

        private IEnumerable<Type> GameEnumerateTypesHelper()
        {
            return loadedAssemblies.SelectMany(assembly => assembly.Value.GetTypes().Where(x => typeof(Game).IsAssignableFrom(x)));
        }

        private DebugAssembly CreateDebugAssembly(Assembly assembly)
        {
            var debugAssembly = new DebugAssembly(++currentDebugAssemblyIndex);
            loadedAssemblies.Add(debugAssembly, assembly);
            return debugAssembly;
        }

        public void MainLoop(IGameDebuggerHost gameDebuggerHost)
        {
            host = gameDebuggerHost;
            host.RegisterTarget();
            while (!requestedExit)
            {
                Thread.Sleep(10);
            }
        }
    }
}