// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// This abstract class is an implementation of <see cref="ViewModelBase"/> that uses a dispatcher to invoke
    /// <see cref="OnPropertyChanging"/> and <see cref="OnPropertyChanged"/> handlers.
    /// </summary>
    public abstract class DispatcherViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> to use for this view model.</param>
        protected DispatcherViewModel(IViewModelServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Dispatcher = serviceProvider.Get<IDispatcherService>();
        }

        /// <summary>
        /// Gets a dispatcher that is capable of executing code in the UI thread.
        /// </summary>
        [DataMemberIgnore]
        public IDispatcherService Dispatcher { get; }

        /// <inheritdoc/>
        protected override void OnPropertyChanging(params string[] propertyNames)
        {
            if (HasPropertyChangingSubscriber)
                Dispatcher.Invoke(() => base.OnPropertyChanging(propertyNames));
            else
                base.OnPropertyChanging(propertyNames);
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(params string[] propertyNames)
        {
            if (HasPropertyChangedSubscriber)
                Dispatcher.Invoke(() => base.OnPropertyChanged(propertyNames));
            else
                base.OnPropertyChanged(propertyNames);
        }
    }
}