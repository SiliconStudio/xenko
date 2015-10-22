// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NShader;
using NuGet;
using SiliconStudio.Assets;

namespace SiliconStudio.Paradox.VisualStudio.Commands
{
    /// <summary>
    /// Proxies commands to real <see cref="IParadoxCommands"/> implementation.
    /// </summary>
    public class ParadoxCommandsProxy : MarshalByRefObject, IParadoxCommands
    {
        public static readonly Version MinimumVersion = new Version(1, 1);

        public struct PackageInfo
        {
            public string SdkPath;

            public Version ExpectedVersion;

            public Version LoadedVersion;
        }

        private static object computedParadoxPackageInfoLock = new object();
        private static PackageInfo computedParadoxPackageInfo;
        private static string solution;
        private static bool solutionChanged;
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

        public static PackageInfo ParadoxPackageInfo
        {
            get
            {
                lock (computedParadoxPackageInfoLock)
                {
                    if (computedParadoxPackageInfo.SdkPath == null)
                        computedParadoxPackageInfo = FindParadoxSdkDir();

                    return computedParadoxPackageInfo;
                }
            }
        }

        /// <summary>
        /// Set the solution to use, when resolving the package containing the remote commands.
        /// </summary>
        /// <param name="solutionPath">The full path to the solution file.</param>
        /// <param name="domain">The AppDomain to set the solution on, or null the current AppDomain.</param>
        public static void InitialzeFromSolution(string solutionPath, AppDomain domain = null)
        {
            if (domain == null)
            {
                lock (computedParadoxPackageInfoLock)
                {
                    // Set the new solution and clear the package info, so it will be recomputed
                    solution = solutionPath;
                    computedParadoxPackageInfo = new PackageInfo();
                }

                lock (paradoxCommandProxyLock)
                {
                    solutionChanged = true;
                }
            }
            else
            {
                var initializationHelper = (InitializationHelper)domain.CreateInstanceFromAndUnwrap(typeof(InitializationHelper).Assembly.Location, typeof(InitializationHelper).FullName);
                initializationHelper.Initialze(solutionPath);
            }
        }

        private class InitializationHelper : MarshalByRefObject
        {
            public void Initialze(string solutionPath)
            {
                InitialzeFromSolution(solutionPath);
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
                bool shouldReload = currentInstance == null || solutionChanged;
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
                    InitialzeFromSolution(solution, currentAppDomain);
                    currentInstance = CreateProxy(currentAppDomain);
                    currentInstance.Initialize();
                    solutionChanged = false;
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

        public void Initialize(string paradoxSdkDir)
        {
            Initialize();
        }

        public void Initialize()
        {
            remote.Initialize(ParadoxPackageInfo.SdkPath);
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

            // Redirect requests for earlier package versions to the current one
            var assemblyName = new AssemblyName(args.Name);
            if (assemblyName.Name == executingAssembly.GetName().Name)
                return executingAssembly;

            return null;
        }

        private Assembly ParadoxDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var paradoxSdkDir = ParadoxPackageInfo.SdkPath;
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
            if (assemblyName.Name.StartsWith("System") && (assemblyName.Flags & AssemblyNameFlags.Retargetable) != 0)
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
        private static PackageInfo FindParadoxSdkDir()
        {
            // Resolve the sdk version to load from the solution's package
            var packageInfo = new PackageInfo { ExpectedVersion = PackageSessionHelper.GetPackageVersion(solution) };

            // TODO: Maybe move it in some common class somewhere? (in this case it would be included with "Add as link" in VSPackage)
            var paradoxSdkDir = Environment.GetEnvironmentVariable("SiliconStudioParadoxDir");

            // Failed to locate paradox
            if (paradoxSdkDir == null)
                return packageInfo;

            // If we are in a dev directory, assume we have the right version
            if (File.Exists(Path.Combine(paradoxSdkDir, "build\\Paradox.sln")))
            {
                packageInfo.SdkPath = paradoxSdkDir;
                packageInfo.LoadedVersion = packageInfo.ExpectedVersion;
                return packageInfo;
            }

            // Check if we are in a root directory with store/packages facilities
            if (NugetStore.IsStoreDirectory(paradoxSdkDir))
            {
                var store = new NugetStore(paradoxSdkDir);
                IPackage paradoxPackage = null;

                // Try to find the package with the expected version
                if (packageInfo.ExpectedVersion != null && packageInfo.ExpectedVersion >= MinimumVersion)
                    paradoxPackage = store.GetPackagesInstalled(store.MainPackageId).FirstOrDefault(package => GetVersion(package) == packageInfo.ExpectedVersion);

                // If the expected version is not found, get the latest package
                if (paradoxPackage == null)
                    paradoxPackage = store.GetLatestPackageInstalled(store.MainPackageId);

                // If no package was found, return no sdk path
                if (paradoxPackage == null)
                    return packageInfo;

                // Return the loaded version and the sdk path
                var packageDirectory = store.PathResolver.GetPackageDirectory(paradoxPackage);
                packageInfo.LoadedVersion = GetVersion(paradoxPackage);
                packageInfo.SdkPath = Path.Combine(paradoxSdkDir, store.RepositoryPath, packageDirectory);
            }

            return packageInfo;
        }

        private static Version GetVersion(IPackage package)
        {
            var originalVersion = package.Version.Version;
            return new Version(originalVersion.Major, originalVersion.Minor);
        }
    }
}