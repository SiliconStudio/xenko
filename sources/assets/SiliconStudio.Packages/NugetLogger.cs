// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using ILogger = NuGet.ILogger;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Implementation of the <see cref="ILogger"/> interface using our <see cref="IPackagesLogger"/> interface.
    /// </summary>
    internal class NugetLogger : ILogger
    {
        private readonly IPackagesLogger logger;

        /// <summary>
        /// Initialize new instance of NugetLogger.
        /// </summary>
        /// <param name="logger">The <see cref="IPackagesLogger"/> instance to use to implement <see cref="ILogger"/></param>
        public NugetLogger(IPackagesLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Resolution conflict decision.
        /// </summary>
        /// <param name="message">Message to display when there is a conflict.</param>
        /// <returns>Our implementation always ignore conflicts.</returns>
        public NuGet.FileConflictResolution ResolveFileConflict(string message)
        {
            return NuGet.FileConflictResolution.Ignore;
        }

        /// <summary>
        /// Log <paramref name="message"/> and for now ignore <paramref name="args"/>.
        /// </summary>
        /// <param name="level">Level of logging.</param>
        /// <param name="message">Message to log.</param>
        /// <param name="args">Additional arguments for the message log.</param>
        void ILogger.Log(NuGet.MessageLevel level, string message, params object[] args)
        {
            // Interpret message with args.
            StringWriter sw = new StringWriter();
            sw.Write(message, args);

            switch (level)
            {
                case NuGet.MessageLevel.Debug:
                    logger.Log(MessageLevel.Debug, sw.ToString());
                    break;
                case NuGet.MessageLevel.Error:
                    logger.Log(MessageLevel.Error, sw.ToString());
                    break;
                case NuGet.MessageLevel.Info:
                    logger.Log(MessageLevel.Info, sw.ToString());
                    break;
                case NuGet.MessageLevel.Warning:
                    logger.Log(MessageLevel.Warning, sw.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}
