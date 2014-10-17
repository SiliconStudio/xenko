// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Configuration for <see cref="GlobalLogger"/>.
    /// </summary>
    public class LoggerConfig
    {
        /// <summary>
        /// Gets or sets the minimum level to allow logging.
        /// </summary>
        /// <value>The level.</value>
        public LogMessageType Level { get; set; }
    }
}