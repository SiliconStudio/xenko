// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    public class PropertyChangedViewModelActionItem : DirtiableActionItem
    {
        private readonly PropertyChangedActionItem innerActionItem;

        public PropertyChangedViewModelActionItem(string name, object container, IEnumerable<IDirtiable> dirtiables, string propertyName, object previousValue)
            : base(name, dirtiables)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            innerActionItem = new PropertyChangedActionItem(propertyName, container, previousValue);
        }

        /// <summary>
        /// Gets the type of the property's container.
        /// </summary>
        public Type ContainerType => innerActionItem.ContainerType;

        /// <summary>
        /// Gets the name of the property affected by the change.
        /// </summary>
        public string PropertyName => innerActionItem.PropertyName;

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            innerActionItem.Freeze();
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            innerActionItem.Undo();
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}
