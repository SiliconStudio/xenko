using System.Collections.Generic;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// An implementation of <see cref="DirtiableActionItem"/> that just registers some <see cref="IDirtiable"/> to this action.
    /// </summary>
    public sealed class DirtiableEmptyActionItem : DirtiableActionItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableEmptyActionItem"/> class.
        /// </summary>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        public DirtiableEmptyActionItem(IEnumerable<IDirtiable> dirtiables)
            : base("Register dirtiables", dirtiables)
        {
        }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
        }
    }
}
