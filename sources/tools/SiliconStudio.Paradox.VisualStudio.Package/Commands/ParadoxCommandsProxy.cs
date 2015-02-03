// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using EnvDTE;

using NShader;

using SiliconStudio.Assets;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    /// <summary>
    /// Proxies commands to real <see cref="IParadoxCommands"/> implementation.
    /// </summary>
    public class ParadoxCommandsProxy : MarshalByRefObject, IParadoxCommands
    {
        private static object computedParadoxSdkDirLock = new object();
        private static string computedParadoxSdkDir = null;
        private IParadoxCommands remote;
        private List<Tuple<string, DateTime>> assembliesLoaded = new List<Tuple<string, DateTime>>();

        private static readonly object paradoxCommandProxyLock = new object();
        private static ParadoxCommandsProxy currentInstance;
        private static AppDomain currentAppDomain;

        static ParadoxCommandsProxy()
        {
            // This assembly resolve is only used to resolve the GetExecutingAssembly on the Default Domain
            // when casting to ParadoxCommandsProxy in the ParadoxCommandsProxy.GetProxy method
            AppDomain.CurrentDomain.AssemblyResolve += DefaultDomainAssemblyResolve;
        }

        public ParadoxCommandsProxy()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ParadoxDomainAssemblyResolve;
            var assembly = Assembly.Load("SiliconStudio.Paradox.VisualStudio.Commands");
            remote = (IParadoxCommands)assembly.CreateInstance("SiliconStudio.Paradox.VisualStudio.Commands.ParadoxCommands");
        }

        public static string ParadoxSdkDir
        {
            get
            {
                lock (computedParadoxSdkDirLock)
                {
                    if (computedParadoxSdkDir == null)
                        computedParadoxSdkDir = FindParadoxSdkDir();

                    return computedParadoxSdkDir;
                }
            }
        }

        public override object InitializeLifetimeService()
        {
            // See http://stackoverflow.com/questions/5275839/inter-appdomain-communication-problem
            // If this proxy is not used for 6 minutes, it is disconnected and calls to this proxy will fail
            // We return null to allow the service to run for the full live of the appdomain.
            return null;
        }

        /// <summary>
        /// Gets the current proxy.
        /// </summary>
        /// <returns>ParadoxCommandsProxy.</returns>
        public static ParadoxCommandsProxy GetProxy()
        {
            lock (paradoxCommandProxyLock)
            {
                // New instance?
                bool shouldReload = currentInstance == null;
                if (!shouldReload)
                {
                    // Assemblies changed?
                    shouldReload = currentInstance.ShouldReload();
                }

                // If new instance or assemblies changed, reload
                if (shouldReload)
                {
                    currentInstance = null;
                    if (currentAppDomain != null)
                    {
                        try
                        {
                            AppDomain.Unload(currentAppDomain);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format("Unexpected exception when unloading AppDomain for ParadoxCommandsProxy: {0}", ex));
                        }
                    }

                    currentAppDomain = CreateParadoxDomain();
                    currentInstance = CreateProxy(currentAppDomain);
                }

                return currentInstance;
            }
        }

        /// <summary>
        /// Creates the paradox domain.
        /// </summary>
        /// <returns>AppDomain.</returns>
        public static AppDomain CreateParadoxDomain()
        {
            return AppDomain.CreateDomain("paradox-domain");
        }

        /// <summary>
        /// Gets the current proxy.
        /// </summary>
        /// <returns>ParadoxCommandsProxy.</returns>
        public static ParadoxCommandsProxy CreateProxy(AppDomain domain)
        {
            if (domain == null) throw new ArgumentNullException("domain");
            return (ParadoxCommandsProxy)domain.CreateInstanceFromAndUnwrap(typeof(ParadoxCommandsProxy).Assembly.Location, typeof(ParadoxCommandsProxy).FullName);
        }

        public void Initialize()
        {
            remote.Initialize();
        }

        public bool ShouldReload()
        {
            lock (assembliesLoaded)
            {
                // Check if any assemblies have changed since loaded
                foreach (var assemblyItem in assembliesLoaded)
                {
                    var assemblyPath = assemblyItem.Item1;
                    var lastAssemblyTime = assemblyItem.Item2;

                    if (File.Exists(assemblyPath))
                    {
                        var fileDateTime = File.GetLastWriteTime(assemblyPath);
                        if (fileDateTime != lastAssemblyTime)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void StartRemoteBuildLogServer(IBuildMonitorCallback buildMonitorCallback, string logPipeUrl)
        {
            remote.StartRemoteBuildLogServer(buildMonitorCallback, logPipeUrl);
        }

        public byte[] GenerateShaderKeys(string inputFileName, string inputFileContent)
        {
            return remote.GenerateShaderKeys(inputFileName, inputFileContent);
        }

        public byte[] GenerateDataClasses(string assemblyOutput, string projectFullName, string intermediateAssembly)
        {
            return remote.GenerateDataClasses(assemblyOutput, projectFullName, intermediateAssembly);
        }

        public RawShaderNavigationResult AnalyzeAndGoToDefinition(string sourceCode, RawSourceSpan span)
        {
            // TODO: We need to know which package is currently selected in order to query all valid shaders
            return remote.AnalyzeAndGoToDefinition(sourceCode, span);
        }

        private static Assembly DefaultDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // This assembly resolve is only used to resolve the GetExecutingAssembly on the Default Domain
            // when casting to ParadoxCommandsProxy in the ParadoxCommandsProxy.GetProxy method
            var executingAssembly = Assembly.GetExecutingAssembly();
            if (args.Name == executingAssembly.FullName)
                return executingAssembly;

            return null;
        }

        private Assembly ParadoxDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var paradoxSdkDir = ParadoxSdkDir;
            if (paradoxSdkDir == null)
                return null;

            var paradoxSdkBinDir = Path.Combine(paradoxSdkDir, @"Bin\Windows-Direct3D11");

            // Try to load .dll/.exe from Paradox SDK directory
            var assemblyName = new AssemblyName(args.Name);
            var assemblyFile = Path.Combine(paradoxSdkBinDir, assemblyName.Name + ".dll");
            if (File.Exists(assemblyFile))
                return LoadAssembly(assemblyFile);

            assemblyFile = Path.Combine(paradoxSdkBinDir, assemblyName.Name + ".exe");
            if (File.Exists(assemblyFile))
                return LoadAssembly(assemblyFile);

            // PCL System assemblies are using version 2.0.5.0 while we have a 4.0
            // Redirect the PCL to use the 4.0 from the current app domain.
            if (assemblyName.Name.StartsWith("System"))
            {
                var systemCoreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == assemblyName.Name);
                return systemCoreAssembly;
            }

            return null;
        }

        private Assembly LoadAssembly(string assemblyFile)
        {
            lock (assembliesLoaded)
            {
                assembliesLoaded.Add(new Tuple<string, DateTime>(assemblyFile, File.GetLastWriteTime(assemblyFile)));
            }

            // Check if .pdb exists as well
            var pdbFile = Path.ChangeExtension(assemblyFile, "pdb");
            if (File.Exists(pdbFile))
                return Assembly.Load(File.ReadAllBytes(assemblyFile), File.ReadAllBytes(pdbFile));

            // Otherwise load assembly without PDB
            return Assembly.Load(File.ReadAllBytes(assemblyFile));
        }

        /// <summary>
        /// Gets the paradox SDK dir.
        /// </summary>
        /// <returns></returns>
        private static string FindParadoxSdkDir()
        {
            // TODO: Get the Paradox SDK from the current selected package

            // TODO: Maybe move it in some common class somewhere? (in this case it would be included with "Add as link" in VSPackage)
            var paradoxSdkDir = Environment.GetEnvironmentVariable("SiliconStudioParadoxDir");

            if (paradoxSdkDir == null)
            {
                return null;
            }

            // Check if it is a dev directory
            if (File.Exists(Path.Combine(paradoxSdkDir, "build\\Paradox.sln")))
                return paradoxSdkDir;

            // Check if we are in a root directory with store/packages facilities
            if (NugetStore.IsStoreDirectory(paradoxSdkDir))
            {
                var store = new NugetStore(paradoxSdkDir);

                var paradoxPackage = store.GetLatestPackageInstalled(store.MainPackageId);
                if (paradoxPackage == null)
                    return null;

                var packageDirectory = store.PathResolver.GetPackageDirectory(paradoxPackage);
                return Path.Combine(paradoxSdkDir, NugetStore.DefaultGamePackagesDirectory, packageDirectory);
            }

            return null;
        }
    }
}