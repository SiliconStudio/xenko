// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
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
        /// Executes the node command.
        /// </summary>
        /// <param name="currentValue">The current value of the associated object or member.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <param name="undoToken">The <see cref="UndoToken"/> that will be passed to the <see cref="Undo"/> method when undoing the execution of this command.</param>
        /// <returns>The new value to assign to the associated object or member.</returns>
        [Obsolete]
        object Execute(object currentValue, object parameter, out UndoToken undoToken);

        Task<ActionItem> Execute2(IContent content, object index, object parameter);

        Task<ActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables);

        /// <summary>
        /// Undoes an invoke of the node command.
        /// </summary>
        /// <param name="currentValue">The current value of the associated object or member.</param>
        /// <param name="undoToken">The <see cref="UndoToken"/> that was generated when invoking this command.</param>
        /// <param name="redoToken">The <see cref="RedoToken"/> that will be passed to the <see cref="Redo"/> method when redoing the execution of this command.</param>
        /// <returns>The new value to assign to the associated object or member.</returns>
        [Obsolete]
        object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken);

        /// <summary>
        /// Redoes the node command.
        /// </summary>
        /// <param name="currentValue">The current value of the associated object or member.</param>
        /// <param name="redoToken">The <see cref="RedoToken"/> that was generated when undoing this command.</param>
        /// <param name="undoToken">The <see cref="UndoToken"/> that will be passed to the <see cref="Undo"/> method when undoing the execution of this command.</param>
        /// <returns>The new value to assign to the associated object or member.</returns>
        [Obsolete]
        object Redo(object currentValue, RedoToken redoToken, out UndoToken undoToken);

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
