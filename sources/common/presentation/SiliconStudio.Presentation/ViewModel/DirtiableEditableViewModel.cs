// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
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
        protected DirtiableEditableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public bool IsDirty { get { return isDirty; } private set { var oldValue = isDirty; SetValueUncancellable(ref isDirty, value); OnDirtyFlagSet(oldValue, value); } }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiable> Dirtiables => this.Yield();

        protected virtual void OnDirtyFlagSet(bool oldValue, bool newValue)
        {
            // intentionally do nothing
        }
        
        void IDirtiable.UpdateDirtiness(bool value)
        {
            IsDirty = value;
        }
    }
}
