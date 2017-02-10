using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Dirtiables
{
    public interface IDirtyingOperation
    {
        /// <summary>
        /// Gets whether this operation is currently realized.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Gets the dirtiable objects associated to this operation, or <c>null</c> if no dirtiable is associated.
        /// </summary>
        [NotNull]
        IReadOnlyList<IDirtiable> Dirtiables { get; }
    }
}
