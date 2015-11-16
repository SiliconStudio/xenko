// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// An implementation of the <see cref="EditableViewModel"/> that is also itself an <see cref="IDirtiable"/>. The <see cref="Dirtiables"/> 
    /// property returns an enumerable containing the instance itself.
    /// </summary>
    public abstract class DirtiableEditableViewModel : EditableViewModel, IDirtiable, IDisposable
    {
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableEditableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        protected DirtiableEditableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public bool IsDirty { get { return isDirty; } private set { var oldValue = isDirty; SetValueUncancellable(ref isDirty, value); OnDirtyFlagSet(oldValue, value); } }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiable> Dirtiables => this.Yield();

        /// <inheritdoc/>
        public event EventHandler<DirtinessUpdatedEventArgs> DirtinessUpdated;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            var dirtiableManager = ServiceProvider.TryGet<DirtiableManager>();
            dirtiableManager?.UnregisterDirtiableDependency(this, null);
        }

        protected virtual void OnDirtyFlagSet(bool oldValue, bool newValue)
        {
            // intentionally do nothing
        }
        
        void IDirtiable.UpdateDirtiness(bool value)
        {
            bool previousValue = IsDirty;
            IsDirty = value;
            DirtinessUpdated?.Invoke(this, new DirtinessUpdatedEventArgs(previousValue, IsDirty));
        }
    }
}