// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml.Serialization.Logging;
using ILogger = SiliconStudio.Core.Diagnostics.ILogger;

namespace SiliconStudio.Assets.Serializers
{
    public class YamlForwardLogger : Core.Yaml.Serialization.Logging.ILogger
    {
        private readonly ILogger logger;

        public YamlForwardLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void Log(LogLevel level, Exception ex, string message)
        {
            LogMessageType levelConverted;
            switch (level)
            {
                case LogLevel.Error:
                    levelConverted = LogMessageType.Error;
                    break;
                case LogLevel.Warning:
                    levelConverted = LogMessageType.Warning;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }

            // No need to display message for now, usually ex.Message contains enough information
            logger.Log(new LogMessage("Asset", levelConverted, ex.Message));
        }
    }
}