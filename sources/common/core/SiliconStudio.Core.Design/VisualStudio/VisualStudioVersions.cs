// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;

namespace SiliconStudio.Core.VisualStudio
{
    public struct IDEDictionaryKey
    {
        int key;

        public int Key { get => key; set => key = value; }

        public IDEDictionaryKey(int key)
        {
            this.key = key;
        }

        public override int GetHashCode() => key;

        public override bool Equals(object obj)
        {
            if (!(obj is IDEDictionaryKey))
                return false;

            return ((IDEDictionaryKey)obj).Key == key;
        }
    }

    public struct IDEInfo
    {
        public string DisplayName { get; set; }
        public string InstallationPath { get; set; }
    }

    public static class VisualStudioVersions
    {
        private static Dictionary<IDEDictionaryKey, IDEInfo> ideDictionary;

        public const string DefaultIDE = "Default IDE";
        private const string VisualStudio2015 = "Visual Studio 2015";

        private const int maximumSlotSize = 255;    // Workaround for the Configuration lack of proper enumeration

        private static void BuildDictionary()
        {
            if (ideDictionary != null)
                return;

            ideDictionary = new Dictionary<IDEDictionaryKey, IDEInfo>();

            ideDictionary.Add(new IDEDictionaryKey(ideDictionary.Count),
                new IDEInfo { DisplayName = "Default IDE", InstallationPath = null });

            // Visual Studio 14.0 (2015)
            var vs14InstallPath = GetSpecificVisualStudioPath("14.0");
            if (vs14InstallPath != null)
            {
                ideDictionary.Add(new IDEDictionaryKey(ideDictionary.Count),
                    new IDEInfo { DisplayName = "Visual Studio 2015", InstallationPath = vs14InstallPath });
            }

            // Visual Studio 15.0 (2017) and later
            {
                var configuration = new SetupConfiguration();

                var instances = configuration.EnumAllInstances();
                instances.Reset();
                var inst = new ISetupInstance[maximumSlotSize];
                int pceltFetched;
                instances.Next(maximumSlotSize, inst, out pceltFetched);

                for (int i = 0; i < pceltFetched; i++)
                {
                    var path = Path.Combine(inst[i].ResolvePath(), "Common7\\IDE\\devenv.exe");
                    if (path == null)
                        continue;

                    ideDictionary.Add(new IDEDictionaryKey(ideDictionary.Count),
                        new IDEInfo { DisplayName = inst[i].GetDisplayName(), InstallationPath = path });
                }
            }
        }

        public static IEnumerable<string> AvailableVisualStudioVersions
        {
            get
            {
                BuildDictionary();

                foreach (var ideInfo in ideDictionary.Values)
                    yield return ideInfo.DisplayName;
            }
        }

        public static string GetVersionNumber(string visualStudioVersion)
        {
            switch (visualStudioVersion)
            {
                case VisualStudio2015:
                    return ("14.0");
                default:
                    {
                        var configuration = new SetupConfiguration();

                        var instances = configuration.EnumAllInstances();
                        instances.Reset();
                        var inst = new ISetupInstance[maximumSlotSize];
                        int pceltFetched;
                        instances.Next(maximumSlotSize, inst, out pceltFetched);

                        for (int i = 0; i < pceltFetched; i++)
                        {
                            if (visualStudioVersion.Equals(inst[i].GetDisplayName()))
                                return inst[i].GetInstallationVersion();
                        }
                    }
                    return null;
            }
        }

        public static string GetVisualStudioPath(string visualStudioVersion)
        {
            switch (visualStudioVersion)
            {
                case VisualStudio2015:
                    return GetSpecificVisualStudioPath(GetVersionNumber(visualStudioVersion));
                default:
                    return GetGenericVisualStudioPath(visualStudioVersion);
            }
        }

        private static string GetGenericVisualStudioPath(string displayName)
        {
            var configuration = new SetupConfiguration();

            var instances = configuration.EnumAllInstances();
            instances.Reset();
            var inst = new ISetupInstance[maximumSlotSize];
            int pceltFetched;
            instances.Next(maximumSlotSize, inst, out pceltFetched);

            for (int i = 0; i < pceltFetched; i++)
            {
                var path = Path.Combine(inst[i].ResolvePath(), "Common7\\IDE\\devenv.exe");
                if (path == null)
                    continue;

                if (displayName.Equals(inst[i].GetDisplayName()))
                    return path;
            }

            return null;
        }

        private static string GetSpecificVisualStudioPath(string version)
        {
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var subkey = localMachine32.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\{0}\{1}", "VisualStudio", version)))
            {
                if (subkey == null)
                    return null;

                var path = (string)subkey.GetValue("InstallDir");
                if (path == null)
                    return null;

                return Path.Combine(path, "devenv.exe");
            }
        }
    }
}
