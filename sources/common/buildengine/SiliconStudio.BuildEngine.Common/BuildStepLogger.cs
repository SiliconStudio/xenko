// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.BuildEngine
{
    public class BuildStepLogger : Logger
    {
        private readonly ILogger mainLogger;
        public readonly TimestampLocalLogger StepLogger;

        public BuildStepLogger(ILogger mainLogger, DateTime startTime)
        {
            this.mainLogger = mainLogger;
            StepLogger = new TimestampLocalLogger(startTime);
            // Let's receive all level messages, each logger will filter them itself
            ActivateLog(LogMessageType.Debug);
            // StepLogger messages will be forwarded to the monitor, which will also filter itself
            StepLogger.ActivateLog(LogMessageType.Debug);
        }

        protected override void LogRaw(ILogMessage logMessage)
        {
            if (mainLogger != null)
            {
                mainLogger.Log(logMessage);
            }
            if (StepLogger != null)
            {
                lock (StepLogger)
                {
                    StepLogger.Log(logMessage);
                }
            }
        }
    }
}
