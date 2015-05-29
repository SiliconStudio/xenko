// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.ViewModel
{
    public enum ActionStackChange
    {
        Save,
        UndoRedo,
    }

    /// <summary>
    /// An interface that represents an object which can be in a dirty state (modified since last save). This interface provides access to the dirty state and methods to
    /// bind <see cref="ViewModelActionItem"/> in order to update the dirtiness when the action stack is reset to the save marker and/or modified again.
    /// </summary>
    public interface IDirtiableViewModel
    {
        /// <summary>
        /// Gets the dirty state of this object.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Raised when the <see cref="IsDirty"/> property has changed.
        /// </summary>
        event EventHandler<DirtinessUpdatedEventArgs> DirtinessUpdated;

        /// <summary>
        /// Register a <see cref="ViewModelActionItem"/> object to this dirtiable object. A registered action item can modify the dirty state when
        /// its <see cref="ActionItem.IsSaved"/> and/or its <see cref="ActionItem.IsDone"/> properties change.
        /// </summary>
        /// <param name="actionItem">The action item to register.</param>
        /// <exception cref="ArgumentException">The given action item is already registered.</exception>
        void RegisterActionItem(ViewModelActionItem actionItem);

        /// <summary>
        /// Discard a previously registered <see cref="ViewModelActionItem"/>.
        /// </summary>
        /// <param name="actionItem">The action item to discard.</param>
        /// <exception cref="ArgumentException">The given action item is not registered.</exception>
        void DiscardActionItem(ViewModelActionItem actionItem);

        /// <summary>
        /// Notify the <see cref="IDirtiableViewModel"/> that a registered action item has been modified and thus the dirty state must be re-evaluated.
        /// </summary>
        /// <param name="change">The type of change that occurred.</param>
        void NotifyActionStackChange(ActionStackChange change);

        /// <summary>
        /// Register a <see cref="IDirtiableViewModel"/> as a dependency of the current object. When a registered dependency object becomes dirty, the current object also become dirty.
        /// </summary>
        /// <param name="dirtiable">The dirtiable object to register as a dependency.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is already registered.</exception>
        void RegisterDirtiableDependency(IDirtiableViewModel dirtiable);

        /// <summary>
        /// Unregister a <see cref="IDirtiableViewModel"/> as a dependency of the current object.
        /// </summary>
        /// <param name="dirtiable">The dirtiable object to unregister.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is not registered.</exception>
        void UnregisterDirtiableDependency(IDirtiableViewModel dirtiable);
    }
}