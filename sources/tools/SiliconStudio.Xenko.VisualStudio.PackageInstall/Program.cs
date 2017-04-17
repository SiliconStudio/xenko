using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.VisualStudio;

namespace SiliconStudio.Xenko.VisualStudio.PackageInstall
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
            var vsixFile = "SiliconStudio.Xenko.vsix";
            switch (args[0])
            {
                case "/install":
                case "/repair":
                {
                    // Start from latest versions first
                    foreach (var visualStudioVersion in VisualStudioVersions.AvailableVisualStudioVersions.Reverse())
                    {
                        if (visualStudioVersion.VsixInstallerPath != null && File.Exists(visualStudioVersion.VsixInstallerPath))
                        {
                            var exitCode = RunVsixInstaller(visualStudioVersion.VsixInstallerPath, "\"" + vsixFile + "\"");
                            if (exitCode != 0)
                                throw new InvalidOperationException($"VSIX Installer didn't run properly: exit code {exitCode}");
                        }
                    }
                    break;
                }
                case "/uninstall":
                    // Start from latest versions first
                    foreach (var visualStudioVersion in VisualStudioVersions.AvailableVisualStudioVersions.Reverse())
                    {
                        if (visualStudioVersion.VsixInstallerPath != null && File.Exists(visualStudioVersion.VsixInstallerPath))
                        {
                            var exitCode = RunVsixInstaller(visualStudioVersion.VsixInstallerPath, "/uninstall:b0b8feb1-7b83-43fc-9fc0-70065ddb80a1");
                            if (exitCode != 0)
                                throw new InvalidOperationException($"VSIX Installer didn't run properly: exit code {exitCode}");
                        }
                    }
                    break;
            }

            return 0;
        }

        /// <summary>
        /// Starts the VSIX installer at the given path with the given argument, and waits for the process to exit before returning.
        /// </summary>
        /// <param name="pathToVsixInstaller">The path to a VSIX installer provided by a version of Visual Studio.</param>
        /// <param name="arguments">The arguments to pass to the VSIX installer.</param>
        /// <returns><c>True</c> if the VSIX installer exited with code 0, <c>False</c> otherwise.</returns>
        private static int RunVsixInstaller(string pathToVsixInstaller, string arguments)
        {
            var process = Process.Start(pathToVsixInstaller, arguments);
            if (process == null)
            {
                return -1;
            }
            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
