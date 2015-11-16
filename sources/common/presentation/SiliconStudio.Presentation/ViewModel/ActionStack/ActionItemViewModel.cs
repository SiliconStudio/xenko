// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    /// <summary>
    /// This class is a view model for object that implements the <see cref="IActionItem"/> interface. It inherits from the <see cref="DispatcherViewModel"/> class.
    /// </summary>
    public class ActionItemViewModel : DispatcherViewModel
    {
        private bool isSavePoint;
        private bool isDone = true;
        private bool isFrozen;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemViewModel"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="IDispatcherService"/> to use for this view model.</param>
        /// <param name="actionItem">The action item linked to this view model.</param>
        public ActionItemViewModel(IViewModelServiceProvider serviceProvider, IActionItem actionItem)
            : base(serviceProvider)
        {
            if (actionItem == null)
                throw new ArgumentNullException(nameof(actionItem));

            ActionItem = actionItem;
            DisplayName = actionItem.Name;
            Refresh();
        }

        /// <summary>
        /// Gets the display name of this action item.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets whether this action item is currently done.
        /// </summary>
        public bool IsDone { get { return isDone; } private set { SetValue(ref isDone, value); } }

        /// <summary>
        /// Gets whether this action item is the current save point.
        /// </summary>
        public bool IsSavePoint { get { return isSavePoint; } internal set { SetValue(ref isSavePoint, value); } }

        /// <summary>
        /// Gets whether this action item has been frozen.
        /// </summary>
        public bool IsFrozen { get { return isFrozen; } private set { SetValue(ref isFrozen, value); } }

        /// <summary>
        /// Gets the action item linked to this view model
        /// </summary>
        internal IActionItem ActionItem { get; }

        /// <summary>
        /// Refreshes the properties of this view model according to the values of the linked action item.
        /// </summary>
        internal void Refresh()
        {
            IsDone = ActionItem.IsDone;
            IsFrozen = ActionItem.IsFrozen;
        }
    }
}
