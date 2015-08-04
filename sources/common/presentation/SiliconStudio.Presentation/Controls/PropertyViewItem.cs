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
    /// <summary>
    /// This class represents an item container of the <see cref="PropertyView"/> control.
    /// </summary>
    public class PropertyViewItem : HeaderedItemsControl
    {
        private readonly ObservableList<PropertyViewItem> properties = new ObservableList<PropertyViewItem>();
        private readonly PropertyView  propertyView;

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(false, OnIsExpandedChanged));

        /// <summary>
        /// Identifies the <see cref="Highlightable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HighlightableProperty = DependencyProperty.Register("Highlightable", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsHighlighted"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey IsHighlightedPropertyKey = DependencyProperty.RegisterReadOnly("IsHighlighted", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsHovered"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey IsHoveredPropertyKey = DependencyProperty.RegisterReadOnly("IsHovered", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsKeyboardActive"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey IsKeyboardActivePropertyKey = DependencyProperty.RegisterReadOnly("IsKeyboardActive", typeof(bool), typeof(PropertyViewItem), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="Offset"/> dependency property.
        /// </summary>
        public static readonly DependencyPropertyKey OffsetPropertyKey = DependencyProperty.RegisterReadOnly("Offset", typeof(double), typeof(PropertyViewItem), new FrameworkPropertyMetadata(0.0));

        /// <summary>
        /// Identifies the <see cref="Increment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(double), typeof(PropertyViewItem), new FrameworkPropertyMetadata(0.0, OnIncrementChanged));

        /// <summary>
        /// Identifies the <see cref="Expanded"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent("Expanded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyViewItem));

        /// <summary>
        /// Identifies the <see cref="Collapsed"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent("Collapsed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyViewItem));

        static PropertyViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyViewItem), new FrameworkPropertyMetadata(typeof(PropertyViewItem)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyViewItem"/> class.
        /// </summary>
        /// <param name="propertyView">The <see cref="PropertyView"/> instance in which this <see cref="PropertyViewItem"/> is contained.</param>
        public PropertyViewItem(PropertyView propertyView)
        {
            if (propertyView == null) throw new ArgumentNullException("propertyView");
            this.propertyView = propertyView;
            PreviewMouseMove += propertyView.ItemMouseMove;
            IsKeyboardFocusWithinChanged += propertyView.OnIsKeyboardFocusWithinChanged;
        }

        /// <summary>
        /// Gets the <see cref="PropertyView"/> control containing this instance of <see cref="PropertyViewItem"/>.
        /// </summary>
        public PropertyView PropertyView { get { return propertyView; } }

        /// <summary>
        /// Gets the collection of <see cref="PropertyViewItem"/> instance contained in this control.
        /// </summary>
        public IReadOnlyCollection<PropertyViewItem> Properties { get { return properties; } }

        /// <summary>
        /// Gets or sets whether this control is expanded.
        /// </summary>
        public bool IsExpanded { get { return (bool)GetValue(IsExpandedProperty); } set { SetValue(IsExpandedProperty, value); } }

        /// <summary>
        /// Gets or sets whether this control can be highlighted.
        /// </summary>
        /// <seealso cref="IsHighlighted"/>
        public bool Highlightable { get { return (bool)GetValue(HighlightableProperty); } set { SetValue(HighlightableProperty, value); } }

        /// <summary>
        /// Gets whether this control is highlighted. The control is highlighted when <see cref="IsHovered"/> and <see cref="Highlightable"/> are both <c>true</c>
        /// </summary>
        /// <seealso cref="Highlightable"/>
        /// <seealso cref="IsHovered"/>
        public bool IsHighlighted { get { return (bool)GetValue(IsHighlightedPropertyKey.DependencyProperty); } }

        /// <summary>
        /// Gets whether the mouse cursor is currently over this control.
        /// </summary>
        public bool IsHovered { get { return (bool)GetValue(IsHoveredPropertyKey.DependencyProperty); } }

        /// <summary>
        /// Gets whether this control is the closest control to the control that has the keyboard focus.
        /// </summary>
        public bool IsKeyboardActive { get { return (bool)GetValue(IsKeyboardActivePropertyKey.DependencyProperty); } }

        /// <summary>
        /// Gets the absolute offset of this <see cref="PropertyViewItem"/>.
        /// </summary>
        public double Offset { get { return (double)GetValue(OffsetPropertyKey.DependencyProperty); } private set { SetValue(OffsetPropertyKey, value); } }

        /// <summary>
        /// Gets or set the increment value used to calculate the <see cref="Offset "/>of the <see cref="PropertyViewItem"/> contained in the <see cref="Properties"/> of this control..
        /// </summary>
        public double Increment { get { return (double)GetValue(IncrementProperty); } set { SetValue(IncrementProperty, value); } }

        /// <summary>
        /// Raised when this <see cref="PropertyViewItem"/> is expanded.
        /// </summary>
        public event RoutedEventHandler Expanded { add { AddHandler(ExpandedEvent, value); } remove { RemoveHandler(ExpandedEvent, value); } }

        /// <summary>
        /// Raised when this <see cref="PropertyViewItem"/> is collapsed.
        /// </summary>
        public event RoutedEventHandler Collapsed { add { AddHandler(CollapsedEvent, value); } remove { RemoveHandler(CollapsedEvent, value); } }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            var item = new PropertyViewItem(propertyView) { Offset = Offset + Increment };
            return item;
        }

        /// <inheritdoc/>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is PropertyViewItem;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var container = (PropertyViewItem)element;
            properties.Add(container);
            RaiseEvent(new PropertyViewItemEventArgs(PropertyView.PrepareItemEvent, this, container, item));
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Invoked when this <see cref="PropertyViewItem"/> is expanded. Raises the <see cref="Expanded"/> event.
        /// </summary>
        /// <param name="e">The routed event arguments.</param>
        protected virtual void OnExpanded(RoutedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Invoked when this <see cref="PropertyViewItem"/> is collapsed. Raises the <see cref="Collapsed"/> event.
        /// </summary>
        /// <param name="e">The routed event arguments.</param>
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