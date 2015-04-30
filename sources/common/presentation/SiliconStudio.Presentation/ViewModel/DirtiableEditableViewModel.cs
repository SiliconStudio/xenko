// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.ViewModel.ActionStack;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// An implementation of the <see cref="EditableViewModel"/> that is also itself an <see cref="IDirtiableViewModel"/>. The <see cref="Dirtiables"/> 
    /// property returns an enumerable containing the instance itself.
    /// </summary>
    public class DirtiableEditableViewModel : EditableViewModel, IDirtiableViewModel, IDisposable
    {
        private readonly HashSet<ViewModelActionItem> changes = new HashSet<ViewModelActionItem>();
        private readonly List<IDirtiableViewModel> dependencies = new List<IDirtiableViewModel>();
        private bool isDirty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirtiableEditableViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> and an <see cref="ITransactionalActionStack"/> to use for this view model.</param>
        public DirtiableEditableViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <inheritdoc/>
        public bool IsDirty { get { return isDirty; } protected set { var oldValue = isDirty; SetValueUncancellable(ref isDirty, value); OnDirtyFlagSet(oldValue, value); } }

        /// <inheritdoc/>
        public override IEnumerable<IDirtiableViewModel> Dirtiables { get { return this.Yield(); } }

        /// <inheritdoc/>
        public event EventHandler<DirtinessUpdatedEventArgs> DirtinessUpdated;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            foreach (var dependency in dependencies)
                dependency.DirtinessUpdated -= DependencyDirtinessUpdated;
        }
        
        /// <inheritdoc/>
        public virtual void RegisterActionItem(ViewModelActionItem actionItem)
        {
            if (changes.Contains(actionItem)) throw new ArgumentException(@"The given action item is already registered.", "actionItem");
            changes.Add(actionItem);
            UpdateDirtiness();
        }

        /// <inheritdoc/>
        public virtual void DiscardActionItem(ViewModelActionItem actionItem)
        {
            bool removed = changes.Remove(actionItem);
            if (!removed) throw new ArgumentException(@"The given action item was not registered.", "actionItem");
            UpdateDirtiness();
        }

        /// <inheritdoc/>
        public virtual void NotifyActionStackChange(ActionStackChange change)
        {
            UpdateDirtiness();
        }

        /// <inheritdoc/>
        public void RegisterDirtiableDependency(IDirtiableViewModel dirtiable)
        {
            if (dependencies.Contains(dirtiable)) throw new ArgumentException(@"The given dirtiable object is already registered as a dependency.", "dirtiable");
            dependencies.Add(dirtiable);
            dirtiable.DirtinessUpdated += DependencyDirtinessUpdated;
        }

        /// <inheritdoc/>
        public void UnregisterDirtiableDependency(IDirtiableViewModel dirtiable)
        {
            dirtiable.DirtinessUpdated -= DependencyDirtinessUpdated;
            bool removed = dependencies.Remove(dirtiable);
            if (!removed) throw new ArgumentException(@"The given dirtiable object was not registered as a dependency.", "dirtiable");
        }

        protected virtual void OnDirtyFlagSet(bool oldValue, bool newValue)
        {
            // intentionally do nothing
        }

        private void DependencyDirtinessUpdated(object sender, DirtinessUpdatedEventArgs e)
        {
            UpdateDirtiness();
        }
        
        private void UpdateDirtiness()
        {
            bool previousValue = IsDirty;
            IsDirty = changes.Any(x => x.IsSaved != x.IsDone) || dependencies.Any(x => x.IsDirty);
            var handler = DirtinessUpdated;
            if (handler != null)
            {
                handler(this, new DirtinessUpdatedEventArgs(previousValue, IsDirty));
            }
        }
    }
}