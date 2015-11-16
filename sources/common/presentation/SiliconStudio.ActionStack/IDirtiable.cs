using System;

namespace SiliconStudio.ActionStack
{
    public enum ActionStackChange
    {
        Save,
        UndoRedo,
        Added,
        Discarded,
    }

    /// <summary>
    /// An interface that represents an object which can be in a dirty state (modified since last save). This interface provides access to the dirty state
    /// of the object, as well as methods to bind a <see cref="DirtiableActionItem"/> in order to update the dirtiness when the action stack is reset to
    /// the save marker and/or modified again.
    /// </summary>
    public interface IDirtiable
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
        /// Registers a <see cref="DirtiableActionItem"/> object to this dirtiable object. A registered action item can modify the dirty state when
        /// its <see cref="ActionItem.IsSaved"/> and/or its <see cref="ActionItem.IsDone"/> properties change.
        /// </summary>
        /// <param name="actionItem">The action item to register.</param>
        /// <exception cref="ArgumentException">The given action item is already registered.</exception>
        void RegisterActionItem(DirtiableActionItem actionItem);

        /// <summary>
        /// Discards a previously registered <see cref="DirtiableActionItem"/>.
        /// </summary>
        /// <param name="actionItem">The action item to discard.</param>
        /// <exception cref="ArgumentException">The given action item is not registered.</exception>
        void DiscardActionItem(DirtiableActionItem actionItem);

        /// <summary>
        /// Notifies the <see cref="IDirtiable"/> that a registered action item has been modified and thus the dirty state must be re-evaluated.
        /// </summary>
        /// <param name="change">The type of change that occurred.</param>
        void NotifyActionStackChange(ActionStackChange change);

        /// <summary>
        /// Registers a <see cref="IDirtiable"/> as a dependency of the current object. When a registered dependency object becomes dirty, the current object also become dirty.
        /// </summary>
        /// <param name="dirtiable">The dirtiable object to register as a dependency.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is already registered.</exception>
        void RegisterDirtiableDependency(IDirtiable dirtiable);

        /// <summary>
        /// Unregisters a <see cref="IDirtiable"/> as a dependency of the current object.
        /// </summary>
        /// <param name="dirtiable">The dirtiable object to unregister.</param>
        /// <exception cref="ArgumentException">The given dirtiable object is not registered.</exception>
        void UnregisterDirtiableDependency(IDirtiable dirtiable);
    }
}