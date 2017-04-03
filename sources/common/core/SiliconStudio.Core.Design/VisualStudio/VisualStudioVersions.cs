// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Setup.Configuration;

namespace SiliconStudio.Core.VisualStudio
{
    public static class VisualStudioVersions
    {
        public static readonly string[] KnownVersions =
        {
            VisualStudio2015,
            XamarinStudio
        };

        public const string DefaultIDE = "Default IDE";
        public const string VisualStudio2015 = "Visual Studio 2015";
        public const string XamarinStudio = "Xamarin Studio";

        private const int maximumSlotSize = 255;    // Workaround for the Configuration lack of proper enumeration

        public static IEnumerable<string> AvailableVisualStudioVersions
        {
            get
            {
                // Default is always included first
                yield return DefaultIDE;

                foreach (var visualStudioVersion in KnownVersions)
                {
                    if (GetVisualStudioPath(visualStudioVersion) != null)
                        yield return visualStudioVersion;
                }

                var configuration = new SetupConfiguration();

                var instances = configuration.EnumAllInstances();
                instances.Reset();
                var inst = new ISetupInstance[maximumSlotSize];
                int pceltFetched;
                instances.Next(maximumSlotSize, inst, out pceltFetched);

                for (int i = 0; i < pceltFetched; i++)
                {
                    yield return inst[i].GetDisplayName();
                }
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
                case XamarinStudio:
                    return GetXamarinStudioPath();
                default:
                    return GetGenericVisualStudioPath(visualStudioVersion);
            }
        }

        private static string GetGenericVisualStudioPath(string version)
        {
            var configuration = new SetupConfiguration();

            var instances = configuration.EnumAllInstances();
            instances.Reset();
            var inst = new ISetupInstance[maximumSlotSize];
            int pceltFetched;
            instances.Next(maximumSlotSize, inst, out pceltFetched);

            for (int i = 0; i < pceltFetched; i++)
            {
                if (version.Equals(inst[i].GetDisplayName()))
                    return Path.Combine(inst[i].ResolvePath(), "Common7\\IDE\\devenv.exe");
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

        private static string GetXamarinStudioPath()
        {
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var subkey = localMachine32.OpenSubKey(string.Format(@"SOFTWARE\Xamarin\XamarinStudio")))
            {
                if (subkey == null)
                    return null;

                var path = (string)subkey.GetValue("Path");
                if (path == null)
                    return null;

                return Path.Combine(path, @"bin\XamarinStudio.exe");
            }
        }
    }
}
