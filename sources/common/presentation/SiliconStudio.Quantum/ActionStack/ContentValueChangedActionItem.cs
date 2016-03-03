using System;
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
        protected object NewValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentValueChangedActionItem"/> class.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        /// <param name="content">The <see cref="IContent"/> instance that has changed.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="index">The index of the change if the change occurred on an item of a collection. <c>null</c> otherwise.</param>
        /// <param name="previousValue">The previous value of the content (or the item if the change occurred on an item of a collection).</param>
        /// <param name="newValue">The new value of the content</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        public ContentValueChangedActionItem(string name, IContent content, ContentChangeType changeType, object index, object previousValue, object newValue, IEnumerable<IDirtiable> dirtiables)
            : base(name, dirtiables)
        {
            Content = content;
            ChangeType = changeType;
            PreviousValue = previousValue;
            NewValue = newValue;
            Index = index;
        }

        public ContentChangeType ChangeType { get; protected set; }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            Index = null;
            PreviousValue = null;
            NewValue = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            ApplyUndo(PreviousValue, NewValue, ChangeType);
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            ContentChangeType changeType = ChangeType;
            switch (ChangeType)
            {
                case ContentChangeType.CollectionAdd:
                    changeType = ContentChangeType.CollectionRemove;
                    break;
                case ContentChangeType.CollectionRemove:
                    changeType = ContentChangeType.CollectionAdd;
                    break;
            }
            ApplyUndo(NewValue, PreviousValue, changeType);
        }

        protected void ApplyUndo(object oldValue, object newValue, ContentChangeType type)
        {
            switch (type)
            {
                case ContentChangeType.ValueChange:
                    Content.Update(oldValue, Index);
                    break;
                case ContentChangeType.CollectionAdd:
                    Content.Remove(Index, newValue);
                    break;
                case ContentChangeType.CollectionRemove:
                    Content.Add(Index, oldValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}