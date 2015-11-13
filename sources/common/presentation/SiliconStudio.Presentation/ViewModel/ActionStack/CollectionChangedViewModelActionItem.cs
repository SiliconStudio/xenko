// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    public class CollectionChangedViewModelActionItem : DirtiableActionItem
    {
        private readonly CollectionChangedActionItem innerActionItem;
        private IDispatcherService dispatcher;

        private CollectionChangedViewModelActionItem(string name, IEnumerable<IDirtiable> dirtiables, IDispatcherService dispatcher)
            : base(name, dirtiables)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            this.dispatcher = dispatcher;
        }

        public CollectionChangedViewModelActionItem(string name, IEnumerable<IDirtiable> dirtiables, IList list, NotifyCollectionChangedAction actionToUndo, IReadOnlyCollection<object> items, int index, IDispatcherService dispatcher)
            : this(name, dirtiables, dispatcher)
        {
            innerActionItem = new CollectionChangedActionItem(list, actionToUndo, items, index);
        }

        public CollectionChangedViewModelActionItem(string name, IEnumerable<IDirtiable> dirtiables, IList list, NotifyCollectionChangedEventArgs args, IDispatcherService dispatcher)
            : this(name, dirtiables, dispatcher)
        {
            innerActionItem = new CollectionChangedActionItem(list, args);
        }

        public NotifyCollectionChangedAction ActionToUndo { get { return innerActionItem.ActionToUndo; } }

        public int ItemCount { get { return innerActionItem.ItemCount; } }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            dispatcher = null;
            innerActionItem.Freeze();
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            dispatcher.Invoke(() => innerActionItem.Undo());
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}
