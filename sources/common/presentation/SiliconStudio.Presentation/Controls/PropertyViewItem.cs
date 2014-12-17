// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SiliconStudio.Presentation.Collections;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    public class PropertyViewItem : HeaderedItemsControl
    {
        private readonly ObservableList<PropertyViewItem> properties = new ObservableList<PropertyViewItem>();
        private readonly PropertyView  propertyView;

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(false, OnIsExpandedChanged));

        public static readonly DependencyProperty CanBeHoveredProperty = DependencyProperty.Register("CanBeHovered", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(true));

        public static readonly DependencyPropertyKey OffsetPropertyKey = DependencyProperty.RegisterReadOnly("Offset", typeof(double), typeof(PropertyViewItem), new FrameworkPropertyMetadata(0.0));

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(PropertyViewItem), new FrameworkPropertyMetadata(0.0, OnIncrementChanged));

        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyViewItem));

        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyViewItem));

        static PropertyViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyViewItem), new FrameworkPropertyMetadata(typeof(PropertyViewItem)));
        }

        public PropertyViewItem(PropertyView propertyView)
        {
            if (propertyView == null) throw new ArgumentNullException("propertyView");
            this.propertyView = propertyView;
            PreviewMouseMove += propertyView.ItemMouseMove;
            
        }

        public PropertyView PropertyView { get { return propertyView; } }

        public IReadOnlyCollection<PropertyViewItem> Properties { get { return properties; } }

        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        public bool CanBeHovered { get { return (bool)GetValue(CanBeHoveredProperty); } set { SetValue(CanBeHoveredProperty, value); } }
        
        public double Offset { get { return (double)GetValue(OffsetPropertyKey.DependencyProperty); } private set { SetValue(OffsetPropertyKey, value); } }

        public double Increment { get { return (double)GetValue(IncrementProperty); } set { SetValue(IncrementProperty, value); } }

        public event RoutedEventHandler Expanded { add { AddHandler(ExpandedEvent, value); } remove { RemoveHandler(ExpandedEvent, value); } }

        public event RoutedEventHandler Collapsed { add { AddHandler(CollapsedEvent, value); } remove { RemoveHandler(CollapsedEvent, value); } }

        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new PropertyViewItem(propertyView) { Offset = Offset + Increment };
            return item;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PropertyViewItem;
        }
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!e.Handled && IsEnabled)
            {
                if (Focus())
                {
                    e.Handled = true;
                }
                if (e.ClickCount % 2 == 0)
                {
                    SetCurrentValue(IsExpandedProperty, !IsExpanded);
                    e.Handled = true;
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var container = (PropertyViewItem)element;
            properties.Add(container);
            RaiseEvent(new PropertyViewItemEventArgs(PropertyView.PrepareItemEvent, this, container, item));
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var container = (PropertyViewItem)element;
            RaiseEvent(new PropertyViewItemEventArgs(PropertyView.ClearItemEvent, this, (PropertyViewItem)element, item));
            properties.Remove(container);
            base.ClearContainerForItemOverride(element, item);
        }

        // TODO
        //protected override AutomationPeer OnCreateAutomationPeer()
        //{
        //    return (AutomationPeer)new TreeViewItemAutomationPeer(this);
        //}

        protected virtual void OnExpanded(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        protected virtual void OnCollapsed(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (PropertyViewItem)d;
            var isExpanded = (bool)e.NewValue;
            //ItemsPresenter itemsHostPresenter = item.ItemsHostPresenter;
            //if (itemsHostPresenter != null)
            //{
            //    collapsed.InvalidateMeasure();
            //    MS.Internal.Helper.InvalidateMeasureOnPath((DependencyObject)itemsHostPresenter, (DependencyObject)collapsed, false);
            //}
            //TreeViewItemAutomationPeer itemAutomationPeer = UIElementAutomationPeer.FromElement((UIElement)collapsed) as TreeViewItemAutomationPeer;
            //if (itemAutomationPeer != null)
            //    itemAutomationPeer.RaiseExpandCollapseAutomationEvent((bool)e.OldValue, newValue);
            if (isExpanded)
                item.OnExpanded(new RoutedEventArgs(ExpandedEvent, item));
            else
                item.OnCollapsed(new RoutedEventArgs(CollapsedEvent, item));
        }

        private static void OnIncrementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (PropertyViewItem)d;
            var delta = (double)e.NewValue - (double)e.OldValue;
            var subItems = item.FindVisualChildrenOfType<PropertyViewItem>();
            foreach (var subItem in subItems)
            {
                subItem.Offset += delta;
            }
        }
    }
}