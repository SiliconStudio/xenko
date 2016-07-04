// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Xenko.Assets
{
    [DataContract("Xenko")]
    public sealed class XenkoConfig
    {
        public const string PackageName = "Xenko";

        private const string XamariniOSBuild = @"MSBuild\Xamarin\iOS\Xamarin.iOS.CSharp.targets";
        private const string XamarinAndroidBuild = @"MSBuild\Xamarin\Android\Xamarin.Android.CSharp.targets";

        private static readonly string[] WindowsRuntimeBuild =
        {
            @"MSBuild\Microsoft\WindowsXaml\v12.0\8.1\Microsoft.Windows.UI.Xaml.Common.Targets",
            @"MSBuild\Microsoft\WindowsXaml\v14.0\8.1\Microsoft.Windows.UI.Xaml.Common.Targets",
        };

        private const string Windows10UniversalRuntimeBuild = @"MSBuild\Microsoft\WindowsXaml\v14.0\8.2\Microsoft.Windows.UI.Xaml.Common.Targets";
        private static readonly string ProgramFilesX86 = Environment.GetEnvironmentVariable(Environment.Is64BitOperatingSystem ? "ProgramFiles(x86)" : "ProgramFiles");

        public static readonly PackageVersion LatestPackageVersion = new PackageVersion(XenkoVersion.CurrentAsText);

        public static PackageDependency GetLatestPackageDependency()
        {
            return new PackageDependency(PackageName, new PackageVersionRange()
                {
                    MinVersion = LatestPackageVersion,
                    IsMinInclusive = true
                });
        }

        /// <summary>
        /// Registers the solution platforms supported by Xenko.
        /// </summary>
        internal static void RegisterSolutionPlatforms()
        {
            var solutionPlatforms = new List<SolutionPlatform>();

            // Define CoreCLR configurations
            var coreClrRelease = new SolutionConfiguration("CoreCLR_Release");
            var coreClrDebug = new SolutionConfiguration("CoreCLR_Debug");
            coreClrDebug.IsDebug = true;
            // Add CoreCLR specific properties
            coreClrDebug.Properties.AddRange(new[]
            {
                "<SiliconStudioRuntime Condition=\"'$(SiliconStudioProjectType)' == 'Executable'\">CoreCLR</SiliconStudioRuntime>",
                "<SiliconStudioBuildDirExtension Condition=\"'$(SiliconStudioBuildDirExtension)' == ''\">CoreCLR</SiliconStudioBuildDirExtension>",
                "<DefineConstants>SILICONSTUDIO_RUNTIME_CORECLR;$(DefineConstants)</DefineConstants>"
            });
            coreClrRelease.Properties.AddRange(new[]
            {
                "<SiliconStudioRuntime Condition=\"'$(SiliconStudioProjectType)' == 'Executable'\">CoreCLR</SiliconStudioRuntime>",
                "<SiliconStudioBuildDirExtension Condition=\"'$(SiliconStudioBuildDirExtension)' == ''\">CoreCLR</SiliconStudioBuildDirExtension>",
                "<DefineConstants>SILICONSTUDIO_RUNTIME_CORECLR;$(DefineConstants)</DefineConstants>"
            });

            // Windows
            var windowsPlatform = new SolutionPlatform()
                {
                    Name = PlatformType.Windows.ToString(),
                    IsAvailable = true,
                    Alias = "Any CPU",
                    Type = PlatformType.Windows
                };
            windowsPlatform.PlatformsPart.Add(new SolutionPlatformPart("Any CPU"));
            windowsPlatform.PlatformsPart.Add(new SolutionPlatformPart("Mixed Platforms") { Alias = "Any CPU"});
            windowsPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP");
            windowsPlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsPlatform.Configurations.Add(new SolutionConfiguration("AppStore"));
            windowsPlatform.Configurations.Add(coreClrDebug);
            windowsPlatform.Configurations.Add(coreClrRelease);
            foreach (var part in windowsPlatform.PlatformsPart)
            {
                part.Configurations.Clear();
                part.Configurations.AddRange(windowsPlatform.Configurations);
            }
            solutionPlatforms.Add(windowsPlatform);

            // Windows Store
            var windowsStorePlatform = new SolutionPlatform()
            {
                Name = PlatformType.WindowsStore.ToString(),
                DisplayName = "Windows Store",
                Type = PlatformType.WindowsStore,
                IsAvailable = WindowsRuntimeBuild.Any(IsFileInProgramFilesx86Exist),
                UseWithExecutables = false,
                IncludeInSolution = false,
            };

            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME");
            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_STORE");
            windowsStorePlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsStorePlatform.Configurations.Add(new SolutionConfiguration("AppStore"));
            windowsStorePlatform.Configurations["Release"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["Debug"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["Testing"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["AppStore"].Properties.Add("<NoWarn>;2008</NoWarn>");

            foreach (var cpu in new[] { "x86", "x64", "ARM" })
            {
                var windowsStorePlatformCpu = new SolutionPlatformPart(windowsStorePlatform.Name + "-" + cpu)
                {
                    LibraryProjectName = windowsStorePlatform.Name,
                    ExecutableProjectName = cpu,
                    Cpu = cpu,
                    InheritConfigurations = true,
                    UseWithLibraries = false,
                    UseWithExecutables = true,
                };
                windowsStorePlatformCpu.Configurations.Clear();
                windowsStorePlatformCpu.Configurations.AddRange(windowsStorePlatform.Configurations);

                windowsStorePlatform.PlatformsPart.Add(windowsStorePlatformCpu);
            }

            solutionPlatforms.Add(windowsStorePlatform);

            // Windows 10
            var windows10Platform = new SolutionPlatform()
            {
                Name = PlatformType.Windows10.ToString(),
                DisplayName = "Windows 10",
                Type = PlatformType.Windows10,
                IsAvailable = IsFileInProgramFilesx86Exist(Windows10UniversalRuntimeBuild),
                UseWithExecutables = false,
                IncludeInSolution = false,
            };

            windows10Platform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windows10Platform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME");
            windows10Platform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_10");
            windows10Platform.Configurations.Add(new SolutionConfiguration("Testing"));
            windows10Platform.Configurations.Add(new SolutionConfiguration("AppStore"));
            windows10Platform.Configurations["Release"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windows10Platform.Configurations["Debug"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windows10Platform.Configurations["Testing"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windows10Platform.Configurations["AppStore"].Properties.Add("<NoWarn>;2008</NoWarn>");

            windows10Platform.Configurations["Release"].Properties.Add("<UseDotNetNativeToolchain>true</UseDotNetNativeToolchain>");
            windows10Platform.Configurations["Testing"].Properties.Add("<UseDotNetNativeToolchain>true</UseDotNetNativeToolchain>");
            windows10Platform.Configurations["AppStore"].Properties.Add("<UseDotNetNativeToolchain>true</UseDotNetNativeToolchain>");

            foreach (var cpu in new[] { "x86", "x64", "ARM" })
            {
                var windows10PlatformCpu = new SolutionPlatformPart(windows10Platform.Name + "-" + cpu)
                {
                    LibraryProjectName = windows10Platform.Name,
                    ExecutableProjectName = cpu,
                    Cpu = cpu,
                    InheritConfigurations = true,
                    UseWithLibraries = false,
                    UseWithExecutables = true,
                };
                windows10PlatformCpu.Configurations.Clear();
                windows10PlatformCpu.Configurations.AddRange(windows10Platform.Configurations);

                windows10Platform.PlatformsPart.Add(windows10PlatformCpu);
            }

            solutionPlatforms.Add(windows10Platform);

            // Windows Phone
            var windowsPhonePlatform = new SolutionPlatform()
            {
                Name = PlatformType.WindowsPhone.ToString(),
                DisplayName = "Windows Phone",
                Type = PlatformType.WindowsPhone,
                IsAvailable = WindowsRuntimeBuild.Any(IsFileInProgramFilesx86Exist),
                UseWithExecutables = false,
                IncludeInSolution = false,
            };

            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME");
            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_PHONE");
            windowsPhonePlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsPhonePlatform.Configurations.Add(new SolutionConfiguration("AppStore"));

            windowsPhonePlatform.Configurations["Release"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["Debug"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["Testing"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["AppStore"].Properties.Add("<NoWarn>;2008</NoWarn>");

            foreach (var cpu in new[] { "x86", "ARM" })
            {
                var windowsPhonePlatformCpu = new SolutionPlatformPart(windowsPhonePlatform.Name + "-" + cpu)
                {
                    LibraryProjectName = windowsPhonePlatform.Name,
                    ExecutableProjectName = cpu,
                    Cpu = cpu,
                    InheritConfigurations = true,
                    UseWithLibraries = false,
                    UseWithExecutables = true
                };
                windowsPhonePlatformCpu.Configurations.Clear();
                windowsPhonePlatformCpu.Configurations.AddRange(windowsPhonePlatform.Configurations);

                windowsPhonePlatform.PlatformsPart.Add(windowsPhonePlatformCpu);
            }

            solutionPlatforms.Add(windowsPhonePlatform);

            // Linux
            var linuxPlatform = new SolutionPlatform()
            {
                Name = PlatformType.Linux.ToString(),
                IsAvailable = true,
                Type = PlatformType.Linux,
            };
            linuxPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_LINUX");
            linuxPlatform.Configurations.Add(coreClrRelease);
            linuxPlatform.Configurations.Add(coreClrDebug);
            solutionPlatforms.Add(linuxPlatform);

            // Android
            var androidPlatform = new SolutionPlatform()
            {
                Name = PlatformType.Android.ToString(),
                Type = PlatformType.Android,
                IsAvailable = IsFileInProgramFilesx86Exist(XamarinAndroidBuild)
            };
            androidPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO_MOBILE");
            androidPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_ANDROID");
            androidPlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            androidPlatform.Configurations.Add(new SolutionConfiguration("AppStore"));
            androidPlatform.Configurations["Debug"].Properties.AddRange(new[]
                {
                    "<AndroidUseSharedRuntime>True</AndroidUseSharedRuntime>",
                    "<AndroidLinkMode>None</AndroidLinkMode>",
                });
            androidPlatform.Configurations["Release"].Properties.AddRange(new[]
                {
                    "<AndroidUseSharedRuntime>False</AndroidUseSharedRuntime>",
                    "<AndroidLinkMode>SdkOnly</AndroidLinkMode>",
                });
            androidPlatform.Configurations["Testing"].Properties.AddRange(androidPlatform.Configurations["Release"].Properties);
            androidPlatform.Configurations["AppStore"].Properties.AddRange(androidPlatform.Configurations["Release"].Properties);
            solutionPlatforms.Add(androidPlatform);

            // iOS: iPhone
            var iphonePlatform = new SolutionPlatform()
            {
                Name = PlatformType.iOS.ToString(),
                SolutionName = "iPhone", // For iOS, we need to use iPhone as a solution name
                Type = PlatformType.iOS,
                IsAvailable = IsFileInProgramFilesx86Exist(XamariniOSBuild)
            };
            iphonePlatform.PlatformsPart.Add(new SolutionPlatformPart("iPhoneSimulator"));
            iphonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO_MOBILE");
            iphonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_IOS");
            iphonePlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            iphonePlatform.Configurations.Add(new SolutionConfiguration("AppStore"));
            var iPhoneCommonProperties = new List<string>
                {
                    "<ConsolePause>false</ConsolePause>",
                    "<MtouchUseSGen>True</MtouchUseSGen>",
                    "<MtouchArch>ARMv7, ARMv7s, ARM64</MtouchArch>"
                };

            iphonePlatform.Configurations["Debug"].Properties.AddRange(iPhoneCommonProperties);
            iphonePlatform.Configurations["Debug"].Properties.AddRange(new []
                {
                    "<MtouchDebug>True</MtouchDebug>",
                    "<CodesignKey>iPhone Developer</CodesignKey>",
                    "<MtouchUseSGen>True</MtouchUseSGen>",
                });
            iphonePlatform.Configurations["Release"].Properties.AddRange(iPhoneCommonProperties);
            iphonePlatform.Configurations["Release"].Properties.AddRange(new[]
                {
                    "<CodesignKey>iPhone Developer</CodesignKey>",
                });
            iphonePlatform.Configurations["Testing"].Properties.AddRange(iPhoneCommonProperties);
            iphonePlatform.Configurations["Testing"].Properties.AddRange(new[]
                {
                    "<MtouchDebug>True</MtouchDebug>",
                    "<CodesignKey>iPhone Distribution</CodesignKey>",
                    "<BuildIpa>True</BuildIpa>",
                });
            iphonePlatform.Configurations["AppStore"].Properties.AddRange(iPhoneCommonProperties);
            iphonePlatform.Configurations["AppStore"].Properties.AddRange(new[]
                {
                    "<CodesignKey>iPhone Distribution</CodesignKey>",
                });
            solutionPlatforms.Add(iphonePlatform);

            // iOS: iPhoneSimulator
            var iPhoneSimulatorPlatform = iphonePlatform.PlatformsPart["iPhoneSimulator"];
            iPhoneSimulatorPlatform.Configurations["Debug"].Properties.AddRange(new[]
                {
                    "<MtouchDebug>True</MtouchDebug>",
                    "<MtouchLink>None</MtouchLink>",
                    "<MtouchArch>i386, x86_64</MtouchArch>"
                });
            iPhoneSimulatorPlatform.Configurations["Release"].Properties.AddRange(new[]
                {
                    "<MtouchLink>None</MtouchLink>",
                    "<MtouchArch>i386, x86_64</MtouchArch>"
                });

            AssetRegistry.RegisterSupportedPlatforms(solutionPlatforms);
        }

        internal static bool IsFileInProgramFilesx86Exist(string path)
        {
            return (ProgramFilesX86 != null && File.Exists(Path.Combine(ProgramFilesX86, path)));
        }
    }
}
