// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SiliconStudio.Presentation.Behaviors;

namespace SiliconStudio.Presentation.Legacy
{
    public class DragBehavior : DeferredBehaviorBase<UIElement>
    {
        private bool isMouseDown;
        private bool isDragging;
        private Point dragStartPoint;

        public string DataType
        {
            get { return (string)GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }
        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(
            "DataType",
            typeof(string),
            typeof(DragBehavior));

        public object DragData
        {
            get { return GetValue(DragDataProperty); }
            set { SetValue(DragDataProperty, value); }
        }
        public static readonly DependencyProperty DragDataProperty = DependencyProperty.Register(
            "DragData",
            typeof(object),
            typeof(DragBehavior));

        public bool IsDragEnabled
        {
            get { return (bool)GetValue(IsDragEnabledProperty); }
            set { SetValue(IsDragEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsDragEnabledProperty = DependencyProperty.Register(
            "IsDragEnabled",
            typeof(bool),
            typeof(DragBehavior),
            new PropertyMetadata(true));

        /// <summary>
        /// When this behavior is hosted by an ItemsControl, name of the property in the ItemsControl container type that indicate if the current item is selected, inducing that we must handle the PreviewMouseDown event to prevent deselection/reselection when trying to drag the current selection.
        /// </summary>
        public string IsSelectedPropertyName
        {
            get { return (string)GetValue(IsSelectedPropertyNameProperty); }
            set { SetValue(IsSelectedPropertyNameProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedPropertyNameProperty = DependencyProperty.Register(
            "IsSelectedPropertyName",
            typeof(string),
            typeof(DragBehavior),
            new PropertyMetadata("IsSelected"));

        protected override void OnAttachedOverride()
        {
            AssociatedObject.PreviewMouseLeftButtonDown += OnAssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnAssociatedObjectPreviewMouseLeftButtonUp;
            AssociatedObject.MouseMove += OnAssociatedObjectPreviewMouseMove;
        }

        protected override void OnDetachingOverride()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnAssociatedObjectPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnAssociatedObjectPreviewMouseLeftButtonUp;
            AssociatedObject.MouseMove -= OnAssociatedObjectPreviewMouseMove;
        }

        //private bool DeferMouseEvents = true;
        //private bool DeferingMouseEvents = false;
        //private RoutedEventArgs mouseDownEvent;

        private void OnAssociatedObjectPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            dragStartPoint = e.GetPosition((IInputElement)sender);

            // TODO/Benlitz: quite not satisfied of this solution, it is too hackish...

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                return;

            var itemsControl = sender as ItemsControl;
            var depObj = e.OriginalSource as DependencyObject;  
            // Look if the sender is an ItemsControl
            if (itemsControl != null && depObj != null)
            {
                // If so, get the container of the original source...
                var container = itemsControl.ContainerFromElement(depObj);
                if (container != null)
                {
                    // ... and check whether it is currently selected
                    PropertyInfo isSelectedProperty = container.GetType().GetProperty(IsSelectedPropertyName);
                    if (isSelectedProperty != null && (isSelectedProperty.PropertyType == typeof(bool) || isSelectedProperty.PropertyType == typeof(bool?)))
                    {
                        // and handle the event if so. This prevent deselecting a multi-selection just before dragging it.
                        e.Handled = (bool)isSelectedProperty.GetValue(container);
                    }
                }
            }
        }

        private void OnAssociatedObjectPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void OnAssociatedObjectPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && IsDragEnabled)
            {
                var dragCurrentPoint = e.GetPosition((IInputElement)sender);
                if (isDragging == false
                    && (Math.Abs(dragCurrentPoint.X - dragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(dragCurrentPoint.Y - dragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    var itemsControl = sender as ItemsControl;
                    var depObj = e.OriginalSource as DependencyObject;
                    if (itemsControl != null && depObj != null)
                    {
                        var container = itemsControl.ContainerFromElement(depObj);
                        if (container != null)
                        {
                            StartDrag((DependencyObject)sender);
                        }
                    }
                }
            }
        }

        private void StartDrag(DependencyObject dragSource)
        {
            if (DataType == null)
                throw new InvalidOperationException("DataType is not specified.");

            if (DragData == null)
                throw new InvalidOperationException("DragDropContent is not specified.");

            isDragging = true;
            var dataObject = new DataObject(DataType, DragData);
            var de = DragDrop.DoDragDrop(dragSource, dataObject, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.None);
            isMouseDown = false;
            isDragging = false;
        }
    }
}
