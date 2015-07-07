// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Diagnostics;

namespace SiliconStudio
{
    public partial class ShellHelper
    {
        /// <summary>
        /// Run the process and get the output without deadlocks.
        /// </summary>
        /// <param name="command">The name or path of the command.</param>
        /// <param name="parameters">The parameters of the command.</param>
        /// <returns>The outputs.</returns>
        public static ProcessOutputs RunProcessAndGetOutput(string command, string parameters)
        {
            var outputs = new ProcessOutputs();
            using (var adbProcess = Process.Start(
                new ProcessStartInfo(command, parameters)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }))
            {
                adbProcess.OutputDataReceived += (_, args) => LockProcessAndAddDataToList(adbProcess, outputs.OutputLines, args);
                adbProcess.ErrorDataReceived += (_, args) => LockProcessAndAddDataToList(adbProcess, outputs.OutputErrors, args);
                adbProcess.BeginOutputReadLine();
                adbProcess.BeginErrorReadLine();
                adbProcess.WaitForExit();

                outputs.ExitCode = adbProcess.ExitCode;
            }

            return outputs;
        }

        /// <summary>
        /// Run a process without waiting for its output.
        /// </summary>
        /// <param name="command">The name or path of the command.</param>
        /// <param name="parameters">The parameters of the command.</param>
        public static Process RunProcess(string command, string parameters)
        {
            return Process.Start(
                new ProcessStartInfo(command, parameters)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                });
        }

        /// <summary>
        /// Lock the process and save the string.
        /// </summary>
        /// <param name="process">The current process.</param>
        /// <param name="output">List of saved strings.</param>
        /// <param name="args">arguments of the process.</param>
        private static void LockProcessAndAddDataToList(Process process, List<string> output, DataReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                lock (process)
                {
                    output.Add(args.Data);
                }
            }
        }
    }
}