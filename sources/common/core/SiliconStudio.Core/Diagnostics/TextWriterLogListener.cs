// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// A <see cref="LogListener"/> implementation redirecting its output to a <see cref="TextWriter"/>.
    /// </summary>
    public class TextWriterLogListener : LogListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterLogListener"/> class.
        /// </summary>
        /// <param name="logStream">The log stream.</param>
        public TextWriterLogListener([NotNull] Stream logStream)
        {
            LogWriter = new StreamWriter(logStream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterLogListener"/> class.
        /// </summary>
        /// <param name="logWriter">The log writer.</param>
        public TextWriterLogListener(TextWriter logWriter)
        {
            LogWriter = logWriter;
        }

        /// <summary>
        /// Gets the log writer.
        /// </summary>
        /// <value>The log writer.</value>
        public TextWriter LogWriter { get; }

        protected override void OnLog([NotNull] ILogMessage logMessage)
        {
            lock (LogWriter)
            {
                LogWriter.WriteLine(GetDefaultText(logMessage));
                var exceptionMsg = GetExceptionText(logMessage);
                if (!string.IsNullOrEmpty(exceptionMsg))
                {
                    LogWriter.WriteLine(exceptionMsg);
                }
            }
        }

        protected override void Flush()
        {
            if (UseFlushAsync)
            {
                LogWriter.FlushAsync();    
            }
            else
            {
                LogWriter.Flush();
            }
        }
    }
}