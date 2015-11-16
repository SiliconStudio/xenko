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
        [Obsolete("This event will be removed in a future release")]
        event EventHandler<DirtinessUpdatedEventArgs> DirtinessUpdated;

        /// <summary>
        /// Updates the <see cref="IsDirty"/> property to the given value.
        /// </summary>
        /// <param name="value">The new value for the dirty flag.</param>
        void UpdateDirtiness(bool value);
    }
}