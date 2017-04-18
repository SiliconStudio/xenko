using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Archive;

namespace SiliconStudio.Xenko.PackageInstall
{
    class Program
    {
        static int Main(string[] args)
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
                        using (var prerequisitesInstallerProcess = Process.Start(prerequisitesInstallerPath))
                        {
                            if (prerequisitesInstallerProcess == null)
                            {
                                throw new InvalidOperationException($"Could not execute {prerequisitesInstallerPath}");
                            }
                            prerequisitesInstallerProcess.WaitForExit();
                            if (prerequisitesInstallerProcess.ExitCode != 0)
                                return prerequisitesInstallerProcess.ExitCode;
                        }
                    }
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
