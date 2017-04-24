// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.DotNet.Archive;
using Microsoft.Win32;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Xenko.PackageInstall
{
    class Program
    {
        private static readonly string[] NecessaryVS2017Workloads = new[] { "Microsoft.VisualStudio.Workload.ManagedDesktop" };
        private static readonly string[] NecessaryBuildTools2017Workloads = new[] { "Microsoft.VisualStudio.Workload.MSBuildTools", "Microsoft.Net.Component.4.6.1.TargetingPack" };
        private static readonly Guid NET461TargetingPackProductCode = new Guid("8BC3EEC9-090F-4C53-A8DA-1BEC913040F9");
        private const bool AllowVisualStudioOnly = true; // Somehow this doesn't work well yet, so disabled for now

        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    throw new Exception("Expecting a parameter such as /install, /repair or /uninstall");
                }

                bool isRepair = false;
                switch (args[0])
                {
                    case "/install":
                    {
                        // In repair mode, we check if data.xz exists (means install failed)
                        if (!isRepair || File.Exists(@"..\data.xz"))
                        {
                            var indexedArchive = new IndexedArchive();

                            // Note: there is 2 phases while decompressing: Decompress (LZMA) and Expanding (file copying using index.txt)
                            var progressReport = new XenkoLauncherProgressReport(2);

                            // Extract files from LZMA archive
                            indexedArchive.Extract(@"..\data.xz", @"..", progressReport);

                            File.Delete(@"..\data.xz");
                        }

                        // Run prerequisites installer (if it exists)
                        var prerequisitesInstallerPath = @"..\Bin\Prerequisites\install-prerequisites.exe";
                        if (File.Exists(prerequisitesInstallerPath))
                        {
                            var prerequisitesInstalled = false;
                            while (!prerequisitesInstalled)
                            {
                                try
                                {
                                    var prerequisitesInstallerProcess = Process.Start(prerequisitesInstallerPath);
                                    if (prerequisitesInstallerProcess == null)
                                        throw new InvalidOperationException();
                                    prerequisitesInstallerProcess.WaitForExit();
                                    if (prerequisitesInstallerProcess.ExitCode != 0)
                                        throw new InvalidOperationException();
                                    prerequisitesInstalled = true;
                                }
                                catch
                                {
                                    // We'll enter this if UAC has been declined, but also if it timed out (which is a frequent case
                                    // if you don't stay in front of your computer during the installation.
                                    var result = MessageBox.Show("The installation of prerequisites has been canceled by user or failed to run. Do you want to run it again?", "Error",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                    if (result != DialogResult.Yes)
                                        break;
                                }
                            }
                        }

                        // Make sure we have the proper VS2017/BuildTools prerequisites
                        CheckVisualStudioAndBuildTools();

                        break;
                    }
                    case "/repair":
                    {
                        isRepair = true;
                        goto case "/install";
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
                return 1;
            }
        }

        private static void CheckVisualStudioAndBuildTools()
        {
            // Check if there is any VS2017 installed with necessary workloads
            if (!AllowVisualStudioOnly || !VisualStudioVersions.AvailableVisualStudioVersions.Any(x => NecessaryVS2017Workloads.All(workload => x.PackageVersions.ContainsKey(workload))))
            {
                // Check if there is actually a VS2017+ installed
                var existingVisualStudio2017Install = VisualStudioVersions.AvailableVisualStudioVersions.FirstOrDefault(x => x.PackageVersions.ContainsKey("Microsoft.VisualStudio.Component.CoreEditor"));
                var vsInstallerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio\Installer\vs_installer.exe");
                if (AllowVisualStudioOnly && existingVisualStudio2017Install != null && File.Exists(vsInstallerPath))
                {
                    var vsInstaller = Process.Start(vsInstallerPath, $"modify --passive --norestart --installPath \"{existingVisualStudio2017Install.InstallationPath}\" {string.Join(" ", NecessaryVS2017Workloads.Select(x => $"--add {x}"))}");
                    if (vsInstaller == null)
                        throw new InvalidOperationException("Could not run vs_installer.exe");
                    vsInstaller.WaitForExit();
                }
                else
                {
                    // Otherwise, fallback to vs_buildtools standalone detection and install
                    var buildToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio\2017\BuildTools");
                    var buildToolsRoslynPath = Path.Combine(buildToolsPath, @"MSBuild\15.0\Bin\Roslyn");
                    string buildToolsCommandLine = null;

                    if (Directory.Exists(buildToolsPath))
                    {
                        // Already installed; check if all prerequisites workloads are installed
                        // Ideally would be better if we could query this through API/installer (just like we do for Visual Studio), but VSSetup API only exposes Visual Studio instances, not MSBuild
                        if (!Directory.Exists(buildToolsRoslynPath) || !IsSoftwareInstalled(NET461TargetingPackProductCode))
                        {
                            buildToolsCommandLine = $"modify --wait --passive --norestart --installPath \"{buildToolsPath}\" {string.Join(" ", NecessaryBuildTools2017Workloads.Select(x => $"--add {x}"))}";
                        }
                    }
                    else
                    {
                        // Not installed yet
                        buildToolsCommandLine = $"--wait --passive --norestart {string.Join(" ", NecessaryBuildTools2017Workloads.Select(x => $"--add {x}"))}";
                    }

                    if (buildToolsCommandLine != null)
                    {
                        // Run vs_buildtools again
                        var vsBuildToolsInstaller = Process.Start("vs_buildtools.exe", buildToolsCommandLine);
                        if (vsBuildToolsInstaller == null)
                            throw new InvalidOperationException("Could not run vs_buildtools installer");
                        vsBuildToolsInstaller.WaitForExit();
                    }
                }
            }
        }

        private static bool IsSoftwareInstalled(Guid productCode)
        {
            var localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using (var key = localMachine32.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{{productCode}}}"))
            {
                return key != null;
            }
        }

        // Note: Used ConsoleProgressReport as a source
        class XenkoLauncherProgressReport : IProgress<ProgressReport>
        {
            private string _currentPhase;
            private int _currentPhaseIndex = -1;
            private int _phaseCount;
            private double _lastProgress = -1;
            private Stopwatch _stopwatch;
            private object _stateLock = new object();

            public XenkoLauncherProgressReport(int phaseCount)
            {
                _phaseCount = phaseCount;
            }

            public void Report(ProgressReport value)
            {
                long progress = (long)(100 * ((double)value.Ticks / value.Total));

                if (progress == _lastProgress && value.Phase == _currentPhase)
                {
                    return;
                }
                _lastProgress = progress;

                lock (_stateLock)
                {
                    if (value.Phase == _currentPhase)
                    {
                        if (progress == 100)
                        {
                            Console.WriteLine($"Phase {value.Phase} finished in {_stopwatch.ElapsedMilliseconds} ms");
                        }
                    }
                    else
                    {
                        _currentPhase = value.Phase;
                        _currentPhaseIndex++;
                        _stopwatch = Stopwatch.StartNew();
                    }

                    // We compute global progress with each phase having an equal slice
                    long globalProgress = (_currentPhaseIndex * 100 + progress) / _phaseCount;
                    Console.WriteLine($"[ProgressReport:{globalProgress}%] {value.Phase} {progress}%");
                }
            }
        }
    }
}
