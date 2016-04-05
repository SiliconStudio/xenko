using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Dirtiables
{
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
        /// Updates the <see cref="IsDirty"/> property to the given value.
        /// </summary>
        /// <param name="value">The new value for the dirty flag.</param>
        void UpdateDirtiness(bool value);
    }

    public interface IDirtyingOperation
    {
        /// <summary>
        /// Gets whether this operation is currently realized.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Gets the dirtiable objects associated to this operation, or <c>null</c> if no dirtiable is associated.
        /// </summary>
        IReadOnlyCollection<IDirtiable> Dirtiables { get; }
    }

    /// <summary>
    /// An abstact class that inherits from <see cref="ActionItem"/> and can synchronize the dirty status of am <see cref="SiliconStudio.ActionStack.IDirtiable"/> object.
    /// </summary>
    public abstract class DirtiableActionItem : ActionItem
    {
        private readonly List<IDirtiable> dirtiables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableActionItem"/> class with the specified name and dirtiable object.
        /// </summary>
        /// <param name="name">The name of the action item.</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        protected DirtiableActionItem(string name, IEnumerable<IDirtiable> dirtiables)
            : base(name)
        {
            if (dirtiables == null) throw new ArgumentNullException(nameof(dirtiables));
            this.dirtiables = dirtiables.ToList();
        }

        /// <summary>
        /// Gets the dirtiable view model associated to this object, or <c>null</c> if no dirtiable is associated.
        /// </summary>
        public IReadOnlyCollection<IDirtiable> Dirtiables => dirtiables;
    }

}
