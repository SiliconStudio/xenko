// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
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

        static ParadoxCommandsProxy()
        {
            AppDomain.CurrentDomain.AssemblyResolve += domain_AssemblyResolve;
        }

        public ParadoxCommandsProxy()
        {
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

        public static AppDomain CreateAppDomain()
        {
            return AppDomain.CreateDomain("paradox-domain");
        }

        public static ParadoxCommandsProxy CreateProxy(AppDomain domain)
        {
            bool createDomain = domain == null;
            if (createDomain)
                domain = CreateAppDomain();

            try
            {
                return (ParadoxCommandsProxy)domain.CreateInstanceFromAndUnwrap(typeof(ParadoxCommandsProxy).Assembly.Location, typeof(ParadoxCommandsProxy).FullName);
            }
            catch
            {
                if (createDomain)
                    AppDomain.Unload(domain);
                throw;
            }
        }

        static Assembly LoadAssembly(string assemblyFile, bool shadowMemoryCopy)
        {
            if (shadowMemoryCopy)
            {
                // Check if .pdb exists as well
                var pdbFile = Path.ChangeExtension(assemblyFile, "pdb");
                if (File.Exists(pdbFile))
                    return Assembly.Load(File.ReadAllBytes(assemblyFile), File.ReadAllBytes(pdbFile));

                // Otherwise load assembly without PDB
                return Assembly.Load(File.ReadAllBytes(assemblyFile));
            }

            // Load from HDD directly
            return Assembly.Load(assemblyFile);
        }

        static Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == Assembly.GetExecutingAssembly().FullName)
                return Assembly.GetExecutingAssembly();

            var paradoxSdkDir = ParadoxSdkDir;
            if (paradoxSdkDir == null)
                return null;

            var paradoxSdkBinDir = Path.Combine(paradoxSdkDir, @"Bin\Windows-Direct3D11");

            // Try to load .dll/.exe from Paradox SDK directory
            var assemblyName = new AssemblyName(args.Name);
            var assemblyFile = Path.Combine(paradoxSdkBinDir, assemblyName.Name + ".dll");
            if (File.Exists(assemblyFile))
                return LoadAssembly(assemblyFile, true);

            assemblyFile = Path.Combine(paradoxSdkBinDir, assemblyName.Name + ".exe");
            if (File.Exists(assemblyFile))
                return LoadAssembly(assemblyFile, true);

            return null;
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

        /// <summary>
        /// Gets the paradox SDK dir.
        /// </summary>
        /// <returns></returns>
        private static string FindParadoxSdkDir()
        {
            // TODO: Maybe move it in some common class somewhere? (in this case it would be included with "Add as link" in VSPackage)
            var paradoxSdkDir = Environment.GetEnvironmentVariable("SiliconStudioParadoxDir");

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