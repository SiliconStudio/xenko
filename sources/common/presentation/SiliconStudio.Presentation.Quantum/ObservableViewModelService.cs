// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Quantum
{
    /// <summary>
    /// A class that provides various services to <see cref="ObservableViewModel"/> objects
    /// </summary>
    public class ObservableViewModelService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        public ObservableViewModelService()
            : this(x => null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableViewModelService"/> class.
        /// </summary>
        /// <param name="viewModelProvider">A function that returns an <see cref="ObservableViewModel"/> for an given <see cref="ObservableViewModelIdentifier"/>.</param>
        public ObservableViewModelService(Func<ObservableViewModelIdentifier, ObservableViewModel> viewModelProvider)
        {
            if (viewModelProvider == null) throw new ArgumentNullException("viewModelProvider");
            ViewModelProvider = viewModelProvider;
        }

        /// <summary>
        /// Gets or sets a method that retrieves the currently active <see cref="ObservableViewModel"/>. This method is used to get the current observable
        /// view model matching a Quantum object when using undo/redo features, since observable objects can be destroyed and recreated frequently.
        /// </summary>
        public Func<ObservableViewModelIdentifier, ObservableViewModel> ViewModelProvider { get; set; }
    }
}
