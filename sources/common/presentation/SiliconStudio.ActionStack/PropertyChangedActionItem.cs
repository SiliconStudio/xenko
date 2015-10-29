// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;

namespace SiliconStudio.ActionStack
{
    public class PropertyChangedActionItem : ActionItem
    {
        private readonly bool nonPublic;
        private object container;
        private object previousValue;

        public PropertyChangedActionItem(string propertyName, object container, object previousValue, bool nonPublic = false)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (container == null) throw new ArgumentNullException(nameof(container));
            this.PropertyName = propertyName;
            this.container = container;
            this.ContainerType = container.GetType();
            this.previousValue = previousValue;
            this.nonPublic = nonPublic;
        }

        /// <summary>
        /// Gets the type of the property's container.
        /// </summary>
        public Type ContainerType { get; }

        /// <summary>
        /// Gets the name of the property affected by the change.
        /// </summary>
        public string PropertyName { get; }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            container = null;
            previousValue = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            var flags = nonPublic ? BindingFlags.NonPublic | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance;
            var propertyInfo = ContainerType.GetProperty(PropertyName, flags);
            var value = previousValue;
            previousValue = propertyInfo.GetValue(container);
            propertyInfo.SetValue(container, value);
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            UndoAction();
        }
    }
}