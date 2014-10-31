// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SiliconStudio.Presentation.Collections;

namespace SiliconStudio.Presentation.Controls
{
    public class PropertyView : ItemsControl
    {
        private readonly ObservableList<PropertyViewItem> properties = new ObservableList<PropertyViewItem>();
        
        /// <summary>
        /// Identifies the PreparePropertyItem event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a PropertyItemBase containing sub-items.
        /// </summary>
        public static readonly RoutedEvent PrepareItemEvent = EventManager.RegisterRoutedEvent("PrepareItem", RoutingStrategy.Bubble, typeof(PropertyViewItemEventHandler), typeof(PropertyView));

        /// <summary>
        /// Identifies the ClearPropertyItem event.
        /// This attached routed event may be raised by the PropertyGrid itself or by a
        /// PropertyItemBase containing sub items.
        /// </summary>
        public static readonly RoutedEvent ClearItemEvent = EventManager.RegisterRoutedEvent("ClearItem", RoutingStrategy.Bubble, typeof(PropertyViewItemEventHandler), typeof(PropertyView));

        static PropertyView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyView), new FrameworkPropertyMetadata(typeof(PropertyView)));
        }

        public IReadOnlyCollection<PropertyViewItem> Properties { get { return properties; } }

        /// <summary>
        /// This event is raised when a property item is about to be displayed in the PropertyGrid.
        /// This allow the user to customize the property item just before it is displayed.
        /// </summary>
        public event PropertyViewItemEventHandler PrepareItem { add { AddHandler(PrepareItemEvent, value); } remove { RemoveHandler(PrepareItemEvent, value); } }

        /// <summary>
        /// This event is raised when an property item is about to be remove from the display in the PropertyGrid
        /// This allow the user to remove any attached handler in the PreparePropertyItem event.
        /// </summary>
        public event PropertyViewItemEventHandler ClearItem { add { AddHandler(ClearItemEvent, value); } remove { RemoveHandler(ClearItemEvent, value); } }
        
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PropertyViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PropertyViewItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var container = (PropertyViewItem)element;
            properties.Add(container);
            RaiseEvent(new PropertyViewItemEventArgs(PrepareItemEvent, this, container, item));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var container = (PropertyViewItem)element;
            RaiseEvent(new PropertyViewItemEventArgs(ClearItemEvent, this, (PropertyViewItem)element, item));
            properties.Remove(container);
            base.ClearContainerForItemOverride(element, item);
        }

        //protected override AutomationPeer OnCreateAutomationPeer()
        //{
        //    return (AutomationPeer)new TreeViewAutomationPeer(this);
        //}
    }
}
