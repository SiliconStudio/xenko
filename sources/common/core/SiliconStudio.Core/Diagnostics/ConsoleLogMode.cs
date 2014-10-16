// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// Defines how the console is opened.
    /// </summary>
    public enum ConsoleLogMode
    {
        /// <summary>
        /// The console should be visible only in debug and if there is a message, otherwise it is not visible.
        /// </summary>
        Auto,

        /// <summary>
        /// Same as <see cref="Auto"/>
        /// </summary>
        Default = Auto,

        /// <summary>
        /// The console should not be visible.
        /// </summary>
        None,

        /// <summary>
        /// The console should be always visible
        /// </summary>
        Always,
    }
}