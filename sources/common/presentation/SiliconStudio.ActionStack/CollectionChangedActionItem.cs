// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SiliconStudio.ActionStack
{
    public class CollectionChangedActionItem : ActionItem
    {
        private readonly int index;
        private IList list;
        private IReadOnlyCollection<object> items;
        private NotifyCollectionChangedAction actionToUndo;

        private CollectionChangedActionItem(IList list, NotifyCollectionChangedAction actionToUndo)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (actionToUndo == NotifyCollectionChangedAction.Reset) throw new ArgumentException("Reset is not supported by the undo stack.");
            this.list = list;
            this.actionToUndo = actionToUndo;
        }

        public CollectionChangedActionItem(IList list, NotifyCollectionChangedAction actionToUndo, IReadOnlyCollection<object> items, int index)
            : this(list, actionToUndo)
        {
            if (items == null) throw new ArgumentNullException("items");
            this.items = items;
            this.index = index;
        }

        public CollectionChangedActionItem(IList list, NotifyCollectionChangedEventArgs args)
            : this(list, args.Action)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    items = args.NewItems.Cast<object>().ToArray();
                    index = args.NewStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Intentionally ignored, move in collection are not tracked
                    return;
                case NotifyCollectionChangedAction.Remove:
                    items = args.OldItems.Cast<object>().ToArray();
                    index = args.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    items = args.OldItems.Cast<object>().ToArray();
                    index = args.OldStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Reset is not supported by the undo stack.");
                default:
                    items = new object[] { };
                    index = -1;
                    break;
            }
        }

        public NotifyCollectionChangedAction ActionToUndo { get { return actionToUndo; } }

        public int ItemCount { get { return items.Count; } }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            list = null;
            items = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            int i = 0;
            switch (actionToUndo)
            {
                case NotifyCollectionChangedAction.Add:
                    actionToUndo = NotifyCollectionChangedAction.Remove;
                    for (i = 0; i < items.Count(); ++i)
                    {
                        list.RemoveAt(index);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    actionToUndo = NotifyCollectionChangedAction.Add;
                    foreach (var item in items)
                    {
                        list.Insert(index + i, item);
                        ++i;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var replacedItems = new List<object>();
                    foreach (var item in items)
                    {
                        replacedItems.Add(list[index + i]);
                        list[index + i] = item;
                        ++i;
                    }
                    items = replacedItems;
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException("Move is not supported by the undo stack.");
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException("Reset is not supported by the undo stack.");
            }
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}