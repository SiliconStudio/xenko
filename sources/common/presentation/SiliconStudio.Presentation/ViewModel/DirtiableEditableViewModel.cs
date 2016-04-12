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
    public abstract class DirtiableEditableViewModel : EditableViewModel, IDirtiable, IDisposable
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

        /// <inheritdoc/>
        /// <summary>
        /// Gets whether this view model has been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of the Dispose pattern.
        /// </summary>
        /// <remarks>
        /// Derived class should override this method, implement specific cleanup and then call the base implementation.
        /// See https://msdn.microsoft.com/en-us/library/fs2xkftw(v=vs.110).aspx
        /// </remarks>
        /// <param name="disposing">True if called from the <see cref="Dispose"/> method, False if called from a Finalize (destructor) method.</param>
        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }

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
