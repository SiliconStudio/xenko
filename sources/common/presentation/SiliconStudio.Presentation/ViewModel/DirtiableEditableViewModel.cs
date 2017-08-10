// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Dirtiables;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// An implementation of the <see cref="EditableViewModel"/> that is also itself an <see cref="IDirtiable"/>. The <see cref="Dirtiables"/> 
    /// property returns an enumerable containing the instance itself.
    /// </summary>
    public abstract class DirtiableEditableViewModel : EditableViewModel, IDirtiable
    {
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableEditableViewModel"/> class.
        /// </summary>
        protected DirtiableEditableViewModel([NotNull] IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public bool IsDirty { get => isDirty; private set => SetValueUncancellable(ref isDirty, value, OnDirtyFlagSet); }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiable> Dirtiables => this.Yield();

        protected virtual void OnDirtyFlagSet()
        {
            // intentionally do nothing
        }

        protected virtual void UpdateDirtiness(bool value)
        {
            IsDirty = value;
        }

        void IDirtiable.UpdateDirtiness(bool value)
        {
            UpdateDirtiness(value);
        }
    }
}
