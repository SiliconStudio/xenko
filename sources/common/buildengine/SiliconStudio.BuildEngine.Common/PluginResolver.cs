// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    public class PluginResolver
    {
        public IEnumerable<string> PluginAssemblyLocations { get { return pluginAssemblyLocations; } }

        private readonly List<string> pluginAssemblyLocations = new List<string>();

        private readonly Logger logger;

        public PluginResolver(Logger logger = null)
        {
            this.logger = logger;
        }

        public void Register()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => LoadAssembly(new AssemblyName(e.Name));
        }

        public Assembly LoadAssembly(AssemblyName assemblyName)
        {
            return LoadAssembly(assemblyName.Name);
        }

        public Assembly LoadAssembly(string assemblyName)
        {
            foreach (string pluginLocation in pluginAssemblyLocations)
            {
                if (pluginLocation != assemblyName)
                {
                    string fileName = Path.GetFileNameWithoutExtension(pluginLocation);
                    if (fileName != assemblyName)
                        continue;
                }

                logger?.Debug($"Loading plugin: {pluginLocation}");
                return Assembly.LoadFrom(pluginLocation);
            }
            return null;
        }

        public string FindAssembly(string assemblyFileName)
        {
            foreach (string pluginLocation in pluginAssemblyLocations)
            {
                if (pluginLocation != assemblyFileName)
                {
                    string fileName = Path.GetFileName(pluginLocation);
                    if (fileName != assemblyFileName)
                        continue;
                }

                logger?.Debug($"Loading plugin: {pluginLocation}");
                return pluginLocation;
            }
            return null;
        }

        public void AddPlugin(string filePath)
        {
            pluginAssemblyLocations.Add(filePath);
        }

        public void AddPluginFolder(string folder)
        {
            if (!Directory.Exists(folder))
                return;

            foreach (string filePath in Directory.EnumerateFiles(folder, "*.dll"))
            {
                logger?.Debug($"Detected plugin: {Path.GetFileNameWithoutExtension(filePath)}");
                pluginAssemblyLocations.Add(filePath);
            }
        }
    }
}
