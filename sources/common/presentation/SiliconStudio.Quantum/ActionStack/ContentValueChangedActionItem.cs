using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.ActionStack
{
    /// <summary>
    /// An <see cref="IActionItem"/> that corresponds to a change in an <see cref="IContent"/> instance.
    /// </summary>
    public class ContentValueChangedActionItem : DirtiableActionItem
    {
        protected readonly IContent Content;
        protected object Index;
        protected object PreviousValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentValueChangedActionItem"/> class.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        /// <param name="content">The <see cref="IContent"/> instance that has changed.</param>
        /// <param name="index">The index of the change if the change occurred on an item of a collection. <c>null</c> otherwise.</param>
        /// <param name="previousValue">The previous value of the content (or the item if the change occurred on an item of a collection).</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        public ContentValueChangedActionItem(string name, IContent content, object index, object previousValue, IEnumerable<IDirtiable> dirtiables)
            : base(name, dirtiables)
        {
            this.Content = content;
            PreviousValue = previousValue;
            Index = index;
        }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            Index = null;
            PreviousValue = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            var previousValue = Content.Retrieve(Index);
            Content.Update(PreviousValue, Index);
            PreviousValue = previousValue;
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}