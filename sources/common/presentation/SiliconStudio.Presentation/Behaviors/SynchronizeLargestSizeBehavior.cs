// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Windows;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A behavior that allows to synchronize the width and/or height of FrameworkElements so that each instance has the width/height of the largest one.
    /// </summary>
    /// <remarks>Be careful while using this behavior, since the size of the FraneworkElements will always grow and never shrink.</remarks>
    [Obsolete("This behavior is not maintained and should not be used.")]
    public class SynchronizeLargestSizeBehavior : DeferredBehaviorBase<FrameworkElement>
    {
        private static readonly Dictionary<string, List<FrameworkElement>> ElementGroups = new Dictionary<string, List<FrameworkElement>>();
        private readonly DependencyPropertyWatcher propertyWatcher = new DependencyPropertyWatcher();
        private string groupName;

        /// <summary>
        /// Gets or sets a group name for this instance. Every FrameworkElement of the same group will be synchronized.
        /// </summary>
        public string GroupName { get { return groupName; } set { UnregisterElement(); groupName = value; RegisterElement(); } }

        /// <summary>
        /// Gets or sets whether the width of the FrameworkElements should be synchronized.
        /// </summary>
        public bool SynchronizeWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the height of the FrameworkElements should be synchronized.
        /// </summary>
        public bool SynchronizeHeight { get; set; }

        protected override void OnAttachedOverride()
        {
            propertyWatcher.Attach(AssociatedObject);
            if (SynchronizeWidth)
            {
                propertyWatcher.RegisterValueChangedHandler(FrameworkElement.ActualWidthProperty, WidthChanged);
            }
            if (SynchronizeHeight)
            {
                propertyWatcher.RegisterValueChangedHandler(FrameworkElement.ActualHeightProperty, HeightChanged);
            }
            RegisterElement();
        }

        protected override void OnDetachingOverride()
        {
            propertyWatcher.Detach();
            UnregisterElement();
        }

        private void RegisterElement()
        {
            if (AssociatedObject != null)
            {
                List<FrameworkElement> list = ElementGroups.GetOrCreateValue(GroupName);
                list.Add(AssociatedObject);
                if (SynchronizeWidth)
                {
                    WidthChanged(AssociatedObject, EventArgs.Empty);
                }
                if (SynchronizeHeight)
                {
                    HeightChanged(AssociatedObject, EventArgs.Empty);
                }
            }
        }

        private void UnregisterElement()
        {
            if (AssociatedObject != null)
            {
                List<FrameworkElement> list;
                if (ElementGroups.TryGetValue(GroupName, out list))
                {
                    list.Remove(AssociatedObject);
                }
            }
        }

        private void WidthChanged(object sender, EventArgs e)
        {
            List<FrameworkElement> list;
            if (ElementGroups.TryGetValue(GroupName, out list))
            {
                // Prevent every element to resize every other element
                ElementGroups.Remove(GroupName);
                foreach (var element in list)
                {
                    if (element.ActualWidth < AssociatedObject.ActualWidth)
                        element.Width = AssociatedObject.ActualWidth;
                }
                ElementGroups.Add(GroupName, list);
            }
        }

        private void HeightChanged(object sender, EventArgs e)
        {
            List<FrameworkElement> list;
            if (ElementGroups.TryGetValue(GroupName, out list))
            {
                // Prevent every element to resize every other element
                ElementGroups.Remove(GroupName);
                foreach (var element in list)
                {
                    if (element.ActualHeight < AssociatedObject.ActualHeight)
                        element.Height = AssociatedObject.ActualHeight;
                }
                ElementGroups.Add(GroupName, list);
            }
        }
    }
}