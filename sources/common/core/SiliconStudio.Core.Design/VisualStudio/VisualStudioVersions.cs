// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using Microsoft.VisualStudio.Setup.Configuration;

namespace SiliconStudio.Core.VisualStudio
{
    public class IDEInfo
    {
        public override string ToString() => DisplayName;
        public string DisplayName { get; set; }
        public string InstallationPath { get; set; }
    }

    public static class VisualStudioVersions
    {
        private static Dictionary<string, IDEInfo> ideDictionary;

        public static IDEInfo DefaultIDE = new IDEInfo { DisplayName = "Default IDE", InstallationPath = null };

        private static void BuildDictionary()
        {
            if (ideDictionary != null)
                return;

            ideDictionary = new Dictionary<string, IDEInfo>();

            ideDictionary.Add(DefaultIDE.DisplayName, DefaultIDE);

            // Visual Studio 14.0 (2015)
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var subkey = localMachine32.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\{0}\{1}", "VisualStudio", "14.0")))
            {
                var path = (string)subkey?.GetValue("InstallDir");

                var vs14InstallPath = (path != null) ? Path.Combine(path, "devenv.exe") : null;
                if (vs14InstallPath != null)
                {
                    ideDictionary.Add("Visual Studio 2015", new IDEInfo { DisplayName = "Visual Studio 2015", InstallationPath = vs14InstallPath });
                }
            }

            // Visual Studio 15.0 (2017) and later
            {
                var configuration = new SetupConfiguration();

                var instances = configuration.EnumAllInstances();
                instances.Reset();
                var inst = new ISetupInstance[1];
                int pceltFetched;

                while (true)
                {
                    instances.Next(1, inst, out pceltFetched);
                    if (pceltFetched <= 0)
                        break;

                    var path = Path.Combine(inst[0].ResolvePath(), "Common7\\IDE\\devenv.exe");
                    if (File.Exists(path))
                        ideDictionary.Add(inst[0].GetDisplayName(), new IDEInfo { DisplayName = inst[0].GetDisplayName(), InstallationPath = path });
                } 
            }
        }

        public static IEnumerable<IDEInfo> AvailableVisualStudioVersions
        {
            get
            {
                BuildDictionary();

                foreach (var value in ideDictionary.Values)
                    yield return value;
            }
        }
    }
}
