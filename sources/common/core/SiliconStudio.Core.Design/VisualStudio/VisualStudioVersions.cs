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

        public string VsixInstallerPath { get; set; }
    }

    public static class VisualStudioVersions
    {
        private static List<IDEInfo> ideInfos;

        public static IDEInfo DefaultIDE = new IDEInfo { DisplayName = "Default IDE", InstallationPath = null };

        private static void BuildIDEInfos()
        {
            if (ideInfos != null)
                return;

            ideInfos = new List<IDEInfo>();

            ideInfos.Add(DefaultIDE);

            // Visual Studio 14.0 (2015)
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var subkey = localMachine32.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\{0}\{1}", "VisualStudio", "14.0")))
            {
                var path = (string)subkey?.GetValue("InstallDir");

                var vs14InstallPath = (path != null) ? Path.Combine(path, "devenv.exe") : null;
                if (vs14InstallPath != null && File.Exists(vs14InstallPath))
                {
                    var vsixInstallerPath = Path.Combine(path, "VSIXInstaller.exe");
                    if (!File.Exists(vsixInstallerPath))
                        vsixInstallerPath = null;

                    ideInfos.Add(new IDEInfo { DisplayName = "Visual Studio 2015", InstallationPath = vs14InstallPath, VsixInstallerPath = vsixInstallerPath });
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

                    var idePath = Path.Combine(inst[0].ResolvePath(), "Common7\\IDE");
                    var path = Path.Combine(idePath, "devenv.exe");
                    if (File.Exists(path))
                    {
                        var vsixInstallerPath = Path.Combine(idePath, "VSIXInstaller.exe");
                        if (!File.Exists(vsixInstallerPath))
                            vsixInstallerPath = null;

                        ideInfos.Add(new IDEInfo { DisplayName = inst[0].GetDisplayName(), InstallationPath = path, VsixInstallerPath = vsixInstallerPath });
                    }
                } 
            }
        }

        public static IEnumerable<IDEInfo> AvailableVisualStudioVersions
        {
            get
            {
                BuildIDEInfos();

                return ideInfos;
            }
        }
    }
}
