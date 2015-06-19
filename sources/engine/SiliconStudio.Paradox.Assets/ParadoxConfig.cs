// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using SharpDX.Text;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.VisualStudio;
using SiliconStudio.Paradox.Assets.Textures;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets
{
    [DataContract("Paradox")]
    public sealed class ParadoxConfig
    {
        private const string XamariniOSBuild = @"MSBuild\Xamarin\iOS\Xamarin.iOS.CSharp.targets";
        private const string XamarinAndroidBuild = @"MSBuild\Xamarin\Android\Xamarin.Android.CSharp.targets";
        private const string WindowsRuntimeBuild = @"MSBuild\Microsoft\WindowsXaml\v12.0\8.1\Microsoft.Windows.UI.Xaml.Common.Targets";
        private static readonly string ProgramFilesX86 = Environment.GetEnvironmentVariable(Environment.Is64BitOperatingSystem ? "ProgramFiles(x86)" : "ProgramFiles");

        public static PropertyKey<DisplayOrientation> DisplayOrientation = new PropertyKey<DisplayOrientation>("DisplayOrientation", typeof(ParadoxConfig));

        public static PropertyKey<GraphicsPlatform> GraphicsPlatform = new PropertyKey<GraphicsPlatform>("GraphicsPlatform", typeof(ParadoxConfig));

        public static PropertyKey<TextureQuality> TextureQuality = new PropertyKey<TextureQuality>("TextureQuality", typeof(ParadoxConfig));

        public static readonly PackageVersion LatestPackageVersion = new PackageVersion(ParadoxVersion.CurrentAsText);

        public static PackageDependency GetLatestPackageDependency()
        {
            return new PackageDependency("Paradox", new PackageVersionRange()
                {
                    MinVersion = LatestPackageVersion,
                    IsMinInclusive = true
                });
        }

        /// <summary>
        /// Registers the solution platforms supported by Paradox.
        /// </summary>
        internal static void RegisterSolutionPlatforms()
        {
            var solutionPlatforms = new List<SolutionPlatform>();

            // Windows
            var windowsPlatform = new SolutionPlatform()
                {
                    Name = PlatformType.Windows.ToString(),
                    IsAvailable = true,
                    Alias = "Any CPU",
                    Type = PlatformType.Windows
                };
            windowsPlatform.PlatformsPart.Add(new SolutionPlatformPart("Any CPU") { InheritConfigurations = true });
            windowsPlatform.PlatformsPart.Add(new SolutionPlatformPart("Mixed Platforms") { Alias = "Any CPU"});
            windowsPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP");
            windowsPlatform.Properties[GraphicsPlatform] = Graphics.GraphicsPlatform.Direct3D11;
            windowsPlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsPlatform.Configurations.Add(new SolutionConfiguration("AppStore"));

            foreach (var part in windowsPlatform.PlatformsPart)
            {
                part.Configurations.Clear();
                part.Configurations.AddRange(windowsPlatform.Configurations);
            }
            solutionPlatforms.Add(windowsPlatform);

            var parts = windowsPlatform.GetParts();

            // Windows Store
            var windowsStorePlatform = new SolutionPlatform()
            {
                Name = PlatformType.WindowsStore.ToString(),
                DisplayName = "Windows Store",
                Type = PlatformType.WindowsStore,
                IsAvailable = IsFileInProgramFilesx86Exist(WindowsRuntimeBuild),
                UseWithExecutables = false,
                IncludeInSolution = false,
            };

            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME");
            windowsStorePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_STORE");
            windowsStorePlatform.Properties[GraphicsPlatform] = Graphics.GraphicsPlatform.Direct3D11;
            windowsStorePlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsStorePlatform.Configurations.Add(new SolutionConfiguration("AppStore"));
            windowsStorePlatform.Configurations["Release"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["Debug"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["Testing"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsStorePlatform.Configurations["AppStore"].Properties.Add("<NoWarn>;2008</NoWarn>");

            var windowsStorePlatformx86 = new SolutionPlatformPart(windowsStorePlatform.Name + "-x86")
            {
                LibraryProjectName = windowsStorePlatform.Name,
                ExecutableProjectName = "x86",
                Cpu = "x86",
                InheritConfigurations = true,
                UseWithLibraries = false,
                UseWithExecutables = true,
            };
            windowsStorePlatformx86.Configurations.Clear();
            windowsStorePlatformx86.Configurations.AddRange(windowsStorePlatform.Configurations);

            var windowsStorePlatformx64 = new SolutionPlatformPart(windowsStorePlatform.Name + "-x64")
            {
                LibraryProjectName = windowsStorePlatform.Name,
                ExecutableProjectName = "x64",
                Cpu = "x64",
                InheritConfigurations = true,
                UseWithLibraries = false,
                UseWithExecutables = true
            };
            windowsStorePlatformx64.Configurations.Clear();
            windowsStorePlatformx64.Configurations.AddRange(windowsStorePlatform.Configurations);

            var windowsStorePlatformARM = new SolutionPlatformPart(windowsStorePlatform.Name + "-ARM")
            {
                LibraryProjectName = windowsStorePlatform.Name,
                ExecutableProjectName = "ARM",
                Cpu = "ARM",
                InheritConfigurations = true,
                UseWithLibraries = false,
                UseWithExecutables = true
            };
            windowsStorePlatformARM.Configurations.Clear();
            windowsStorePlatformARM.Configurations.AddRange(windowsStorePlatform.Configurations);

            windowsStorePlatform.PlatformsPart.Add(windowsStorePlatformx86);
            windowsStorePlatform.PlatformsPart.Add(windowsStorePlatformx64);
            windowsStorePlatform.PlatformsPart.Add(windowsStorePlatformARM);
            solutionPlatforms.Add(windowsStorePlatform);

            // Windows Phone
            var windowsPhonePlatform = new SolutionPlatform()
            {
                Name = PlatformType.WindowsPhone.ToString(),
                DisplayName = "Windows Phone",
                Type = PlatformType.WindowsPhone,
                IsAvailable = IsFileInProgramFilesx86Exist(WindowsRuntimeBuild),
                UseWithExecutables = false,
                IncludeInSolution = false,
            };

            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS");
            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME");
            windowsPhonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_WINDOWS_PHONE");
            windowsPhonePlatform.Properties[GraphicsPlatform] = Graphics.GraphicsPlatform.Direct3D11;
            windowsPhonePlatform.Configurations.Add(new SolutionConfiguration("Testing"));
            windowsPhonePlatform.Configurations.Add(new SolutionConfiguration("AppStore"));

            windowsPhonePlatform.Configurations["Release"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["Debug"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["Testing"].Properties.Add("<NoWarn>;2008</NoWarn>");
            windowsPhonePlatform.Configurations["AppStore"].Properties.Add("<NoWarn>;2008</NoWarn>");

            var windowsPhonePlatformx86 = new SolutionPlatformPart(windowsPhonePlatform.Name + "-x86")
            {
                LibraryProjectName = windowsPhonePlatform.Name,
                ExecutableProjectName = "x86",
                Cpu = "x86", 
                InheritConfigurations = true, 
                UseWithLibraries = false, 
                UseWithExecutables = true
            };
            windowsPhonePlatformx86.Configurations.Clear();
            windowsPhonePlatformx86.Configurations.AddRange(windowsPhonePlatform.Configurations);

            var windowsPhonePlatformARM = new SolutionPlatformPart(windowsPhonePlatform.Name + "-ARM")
            {
                LibraryProjectName = windowsPhonePlatform.Name,
                ExecutableProjectName = "ARM",
                Cpu = "ARM", 
                InheritConfigurations = true, 
                UseWithLibraries = false, 
                UseWithExecutables = true
            };
            windowsPhonePlatformARM.Configurations.Clear();
            windowsPhonePlatformARM.Configurations.AddRange(windowsPhonePlatform.Configurations);

            windowsPhonePlatform.PlatformsPart.Add(windowsPhonePlatformx86);
            windowsPhonePlatform.PlatformsPart.Add(windowsPhonePlatformARM);
            solutionPlatforms.Add(windowsPhonePlatform);

            // Android
            var androidPlatform = new SolutionPlatform()
            {
                Name = PlatformType.Android.ToString(),
                Type = PlatformType.Android,
                IsAvailable = IsFileInProgramFilesx86Exist(XamarinAndroidBuild)
            };
            androidPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO");
            androidPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO_MOBILE");
            androidPlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_ANDROID");
            androidPlatform.Properties[GraphicsPlatform] = Graphics.GraphicsPlatform.OpenGLES;
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
            iphonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO");
            iphonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_MONO_MOBILE");
            iphonePlatform.DefineConstants.Add("SILICONSTUDIO_PLATFORM_IOS");
            iphonePlatform.Properties[GraphicsPlatform] = Graphics.GraphicsPlatform.OpenGLES;
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