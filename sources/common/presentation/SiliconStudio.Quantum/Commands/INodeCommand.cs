// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base interface for node commands.
    /// </summary>
    public interface INodeCommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets how to combine this command in a combined view model.
        /// </summary>
        CombineMode CombineMode { get; }

        /// <summary>
        /// Indicates whether this command can be attached to an object or a member with the given descriptors.
        /// </summary>
        /// <param name="typeDescriptor">The <see cref="ITypeDescriptor"/> of the object or the member to attach.</param>
        /// <param name="memberDescriptor">The <see cref="MemberDescriptorBase"/> of the member to attach. This parameter is <c>null</c> when testing on an object that is not a member of another object.</param>
        /// <returns></returns>
        bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor);

        /// <summary>
        /// Triggers the command and returns a task that complete when the command execution is done.
        /// </summary>
        /// <param name="content">The content on which to execute the command.</param>
        /// <param name="index">The index in the content on which to execute the command.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <returns>A task that complete when the command execution is done.</returns>
        /// <remarks>A command can execute asynchronous code and is not guaranteed to complete immediately.</remarks>
        Task Execute(IContent content, Index index, object parameter);

        /// <summary>
        /// Notifies the command that the following invokes will be part of a combined execution (the same command being executed multiple times on multiple objects with the same parameters).
        /// </summary>
        /// <seealso cref="EndCombinedInvoke"/>
        void StartCombinedInvoke();

        /// <summary>
        /// Notifies the command that the combined execution is done.
        /// </summary>
        /// <seealso cref="StartCombinedInvoke"/>
        void EndCombinedInvoke();
    }
}
