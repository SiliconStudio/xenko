// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// Status of a command.
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// The command has not finished yet
        /// </summary>
        NotProcessed,
        /// <summary>
        /// The command was successfully executed
        /// </summary>
        Successful,
        /// <summary>
        /// The command execution failed
        /// </summary>
        Failed,
        /// <summary>
        /// The command was started but cancelled, output is undeterminated
        /// </summary>
        Cancelled,
        /// <summary>
        /// A command may not be triggered if its input data haven't changed since the successful last execution
        /// </summary>
        NotTriggeredWasSuccessful,
        /// <summary>
        /// One of the prerequisite command failed and the command was not executed
        /// </summary>
        NotTriggeredPrerequisiteFailed,
    }
}