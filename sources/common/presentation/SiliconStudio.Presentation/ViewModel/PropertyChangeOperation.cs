using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Dirtiables;

namespace SiliconStudio.Presentation.ViewModel
{
    public class PropertyChangeOperation : DirtyingOperation, IMergeableOperation
    {
        private readonly bool nonPublic;
        private object container;
        private object previousValue;

        public PropertyChangeOperation(string propertyName, object container, object previousValue, IEnumerable<IDirtiable> dirtiables, bool nonPublic = false)
            : base(dirtiables)
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
        public virtual bool CanMerge(IMergeableOperation otherOperation)
        {
            var operation = otherOperation as PropertyChangeOperation;
            if (operation == null)
                return false;

            if (container != operation.container)
                return false;

            if (!HasSameDirtiables(operation))
                return false;

            return true;
        }

        /// <inheritdoc/>
        public virtual void Merge(Operation otherOperation)
        {
            // Nothing to do: we keep our current previousValue and we do not store the newValue.
        }

        /// <inheritdoc/>
        protected override void FreezeContent()
        {
            container = null;
            previousValue = null;
        }

        /// <inheritdoc/>
        protected override void Undo()
        {
            var flags = nonPublic ? BindingFlags.NonPublic | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance;
            var propertyInfo = ContainerType.GetProperty(PropertyName, flags);
            var value = previousValue;
            previousValue = propertyInfo.GetValue(container);
            propertyInfo.SetValue(container, value);
        }

        /// <inheritdoc/>
        protected override void Redo()
        {
            // Once we have un-done, the previous value is updated so Redo is just Undoing the Undo
            Undo();
        }
    }
}
