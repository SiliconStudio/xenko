// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Type of a <see cref="LogMessage" />.
    /// </summary>
    [DataContract]
    public enum LogMessageType
    {
        /// <summary>
        /// A debug message (level 0).
        /// </summary>
        Debug = 0,

        /// <summary>
        /// A verbose message (level 1).
        /// </summary>
        Verbose = 1,

        /// <summary>
        /// An regular info message (level 2).
        /// </summary>
        Info = 2,

        /// <summary>
        /// A warning message (level 3).
        /// </summary>
        Warning = 3,

        /// <summary>
        /// An error message (level 4).
        /// </summary>
        Error = 4,

        /// <summary>
        /// A Fatal error message (level 5).
        /// </summary>
        Fatal = 5,
    }
}
