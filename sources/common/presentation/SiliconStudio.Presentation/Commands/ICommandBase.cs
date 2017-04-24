// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows.Input;

namespace SiliconStudio.Presentation.Commands
{
    /// <summary>
    /// An interface representing an implementation of <see cref="ICommand"/> with additional properties.
    /// </summary>
    public interface ICommandBase : ICommand
    {
        /// <summary>
        /// Indicates whether the command can be executed or not.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Executes the command with a <c>null</c> parameter.
        /// </summary>
        void Execute();
    }
}
