using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    [Description("Run external process")]
    public class ExternalProcessCommand : Command
    {
        /// <inheritdoc/>
        public override string Title { get { string title = "External Process "; try { title += Path.GetFileName(ProcessPath) ?? "[Process]"; } catch { title += "[INVALID PATH]"; } return title; } }

        public string ProcessPath { get; set; }
        public string Arguments { get; set; }

        /// <summary>
        /// An optional return code from the command
        /// </summary>
        public int ExitCode;

        /// <summary>
        /// The spawned process
        /// </summary>
        private Process process;

        private Logger logger;

        protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
        {
            logger = commandContext.Logger;
            if (!File.Exists(ProcessPath))
            {
                logger.Error("Unable to find binary file " + ProcessPath);
                return Task.FromResult(ResultStatus.Failed);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = ProcessPath,
                Arguments = Arguments,
                WorkingDirectory = ".",
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            process = Process.Start(startInfo);
            process.OutputDataReceived += OnOutputDataReceived;
            process.BeginOutputReadLine();
            process.WaitForExit();

            ExitCode = process.ExitCode;

            return Task.FromResult(CancellationToken.IsCancellationRequested ? ResultStatus.Cancelled : (ExitCode == 0 ? ResultStatus.Successful : ResultStatus.Failed));
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Debug(e.Data);
        }

        public override void Cancel()
        {
            process.Kill();
        }

        public override string ToString()
        {
            return "External process " + (ProcessPath ?? "[Process]") + (Arguments != null ? " " + Arguments : "");
        }
    }
}
