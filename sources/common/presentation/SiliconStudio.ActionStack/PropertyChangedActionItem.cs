// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// A <see cref="DirtiableActionItem"/> representing a property change in a container object. It uses reflection to update the related properties during
    /// undo/redo operations.
    /// </summary>
    public class PropertyChangedActionItem : DirtiableActionItem
    {
        private readonly bool nonPublic;
        private object container;
        private object previousValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedActionItem"/> class.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        /// <param name="propertyName">The name of the property that has been modified.</param>
        /// <param name="container">The object containing the property that has been modified.</param>
        /// <param name="previousValue">The previous value of the related property, restored when undoing this action.</param>
        /// <param name="dirtiables">The <see cref="IDirtiable"/> objects that are affected by this action.</param>
        /// <param name="nonPublic">Indicates whether the related property is a private property of the container object.</param>
        public PropertyChangedActionItem(string name, string propertyName, object container, object previousValue, IEnumerable<IDirtiable> dirtiables, bool nonPublic = false)
            : base(name, dirtiables)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (container == null) throw new ArgumentNullException(nameof(container));
            PropertyName = propertyName;
            ContainerType = container.GetType();
            this.container = container;
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