// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;

using Microsoft.Win32;

namespace SiliconStudio.Core.VisualStudio
{
    public static class VisualStudioVersions
    {
        public static readonly string[] KnownVersions =
        {
            VisualStudio2012,
            VisualStudio2013,
            VisualStudio2015,
            VisualStudio15,
            VisualCSharpExpress2012,
            VisualCSharpExpress2013,
            VisualCSharpExpress2015,
            XamarinStudio
        };

        public const string DefaultIDE = "Default IDE";
        public const string VisualStudio2012 = "Visual Studio 2012";
        public const string VisualStudio2013 = "Visual Studio 2013";
        public const string VisualStudio2015 = "Visual Studio 2015";
        public const string VisualStudio15 = "Visual Studio 15";
        public const string VisualCSharpExpress2012 = "Visual C# Express 2012";
        public const string VisualCSharpExpress2013 = "Visual C# Express 2013";
        public const string VisualCSharpExpress2015 = "Visual C# Express 2015";
        public const string XamarinStudio = "Xamarin Studio";

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
            }
        }

        public static string GetVersionNumber(string visualStudioVersion)
        {
            switch (visualStudioVersion)
            {
                case VisualStudio2012:
                case VisualCSharpExpress2012:
                    return ("11.0");
                case VisualStudio2013:
                case VisualCSharpExpress2013:
                    return ("12.0");
                case VisualStudio2015:
                case VisualCSharpExpress2015:
                    return ("14.0");
                case VisualStudio15:
                    return ("15.0");
                default:
                    return null;
            }
        }

        public static bool IsExpressVersion(string visualStudioVersion)
        {
            switch (visualStudioVersion)
            {
                case VisualCSharpExpress2012:
                case VisualCSharpExpress2013:
                case VisualCSharpExpress2015:
                    return true;
                default:
                    return false;
            }
        }

        public static string GetVisualStudioPath(string visualStudioVersion)
        {
            switch (visualStudioVersion)
            {
                case VisualStudio2012:
                case VisualStudio2013:
                case VisualStudio2015:
                case VisualCSharpExpress2012:
                case VisualCSharpExpress2013:
                case VisualCSharpExpress2015:
                case VisualStudio15:
                    return GetSpecificVisualStudioPath(GetVersionNumber(visualStudioVersion), IsExpressVersion(visualStudioVersion));
                case XamarinStudio:
                    return GetXamarinStudioPath();
                default:
                    return null;
            }
        }

        private static string GetSpecificVisualStudioPath(string version, bool express)
        {
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var subkey = localMachine32.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\{0}\{1}", express ? "VCSExpress" : "VisualStudio", version)))
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
