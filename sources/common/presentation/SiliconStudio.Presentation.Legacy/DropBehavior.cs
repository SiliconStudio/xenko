// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

using SiliconStudio.Presentation.Behaviors;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Legacy
{
    public class DropBehavior : Behavior<UIElement>
    {
        private static readonly DependencyProperty TestProperty = DependencyProperty.RegisterAttached(
              "Test",
              typeof(object),
              typeof(UIElement),
              new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender)
            );
        
        private InsertAdorner insertAdorner;

        public string DataType
        {
            get { return (string)GetValue(DataTypeProperty); }
            set { SetValue(DataTypeProperty, value); }
        }
        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(
            "DataType",
            typeof(string),
            typeof(DropBehavior));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(DropBehavior));

        public DropBehavior()
        {
            CommandParentBinding = new Binding();
        }

        public BindingBase CommandParentBinding { get; set; }


        protected override void OnAttached()
        {
            // This should stay inside this DropBehavior (specific to this DataType)
            AssociatedObject.PreviewDragEnter += OnAssociatedObjectPreviewDragEnter;
            AssociatedObject.PreviewDragOver += OnAssociatedObjectPreviewDragOver;
            AssociatedObject.PreviewDragLeave += OnAssociatedObjectPreviewDragLeave;
            AssociatedObject.PreviewDrop += OnAssociatedObjectPreviewDrop;

            // This should be part of the future DropCollectionBehavior
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewDragEnter -= RefuseDrag;
            AssociatedObject.PreviewDragEnter += RefuseDrag;
            AssociatedObject.PreviewDragOver -= RefuseDrag;
            AssociatedObject.PreviewDragOver += RefuseDrag;

            // TEST
            AssociatedObject.PreviewMouseLeftButtonDown += OnAssociatedObjectPreviewMouseLeftButtonDown;
        }

        private void OnAssociatedObjectPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnAssociatedObjectPreviewDragLeave(object sender, DragEventArgs e)
        {
            DestroyAdorners();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewDragEnter -= RefuseDrag;
            AssociatedObject.PreviewDragOver -= RefuseDrag;

            AssociatedObject.PreviewDragEnter -= OnAssociatedObjectPreviewDragOver;
            AssociatedObject.PreviewDragOver -= OnAssociatedObjectPreviewDragOver;
            AssociatedObject.PreviewDragLeave -= OnAssociatedObjectPreviewDragLeave;
            AssociatedObject.PreviewDrop -= OnAssociatedObjectPreviewDrop;
        }
        
        static void RefuseDrag(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void OnAssociatedObjectPreviewDragEnter(object sender, DragEventArgs e)
        {
            OnAssociatedObjectPreviewDragOver(sender, e);

            if (e.Data.GetDataPresent(DataType))
            {
            }
        }

        private void OnAssociatedObjectPreviewDragOver(object sender, DragEventArgs e)
        {
            if (DataType == null)
                throw new InvalidOperationException("DataType is not specified.");

            if (e.Data.GetDataPresent(DataType))
            {
                e.Handled = true;
                e.Effects = DragDropEffects.Copy;

                if (sender is ItemsControl || sender is ItemsPresenter)
                {
                    ItemsControl itemsControl;
                    if (sender is ItemsPresenter)
                        itemsControl = ((ItemsPresenter)sender).FindVisualParentOfType<ItemsControl>();
                    else
                        itemsControl = (ItemsControl)sender;

                    var hoveredItem = GetItemFromY(itemsControl, e.GetPosition(itemsControl).Y);

                    if (hoveredItem != null)
                    {
                        if (insertAdorner == null)
                        {
                            var adornerLayer = AdornerLayer.GetAdornerLayer((Visual)sender);
                            insertAdorner = new InsertAdorner((UIElement)sender, adornerLayer);
                            insertAdorner.IsHitTestVisible = false;
                        }

                        insertAdorner.FocusedElementHeader = ExtractHeader(hoveredItem);
                        insertAdorner.IsTopHalf = IsTopHalf(insertAdorner.FocusedElementHeader, e);
                        insertAdorner.FocusedElement = hoveredItem;
                    }

                    if (insertAdorner != null)
                        insertAdorner.Reevaluate();
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnAssociatedObjectPreviewDrop(object sender, DragEventArgs e)
        {
            if (DataType == null)
                throw new InvalidOperationException("DataType is not specified.");

            DestroyAdorners();

            if (Command == null)
            {
                e.Handled = true;
                return;
            }

            if (e.Data.GetDataPresent(DataType))
            {
                e.Handled = true;

                var dropCommandParameters = new DropCommandParameters
                {
                    DataType = DataType,
                    Data = e.Data.GetData(DataType),
                    SourceIndex = -1,
                    TargetIndex = -1,
                    Sender = sender
                };

                if (sender is ItemsControl || sender is ItemsPresenter)
                {
                    ItemsControl itemsControl;

                    if (sender is ItemsPresenter)
                        itemsControl = ((ItemsPresenter)sender).FindVisualParentOfType<ItemsControl>();
                    else
                        itemsControl = (ItemsControl)sender;

                    var item = GetItemFromY(itemsControl, e.GetPosition(itemsControl).Y);

                    if (item != null)
                    {
                        bool isTopHalf = IsTopHalf(ExtractHeader(item), e);
                        var parent = GetParent(itemsControl, item);

                        var sourceContainer = parent.ItemContainerGenerator.ContainerFromItem(dropCommandParameters.Data);
                        if (sourceContainer != null)
                            dropCommandParameters.SourceIndex = parent.ItemContainerGenerator.IndexFromContainer(sourceContainer);

                        dropCommandParameters.TargetIndex = parent.ItemContainerGenerator.IndexFromContainer(item) + (isTopHalf ? 0 : 1);
                        var bindingExpression = BindingOperations.SetBinding(parent, TestProperty, CommandParentBinding);
                        if (!bindingExpression.HasError && bindingExpression.Status == BindingStatus.Active)
                            dropCommandParameters.Parent = parent.GetValue(TestProperty);
                        BindingOperations.ClearBinding(parent, TestProperty);
                    }
                }

                if (Command.CanExecute(dropCommandParameters))
                    Command.Execute(dropCommandParameters);
            }
        }

        private void DestroyAdorners()
        {
            if (insertAdorner != null)
            {
                insertAdorner.Destroy();
                insertAdorner = null;
            }
        }

        public static ItemsControl GetParent(ItemsControl root, DependencyObject item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent == root || parent is TreeViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

        private static UIElement ExtractHeader(UIElement element)
        {
            // If we have a TreeViewItem, get only its header (since the control contains Children as well)
            if (element is TreeViewItem)
            {
                element = (UIElement)((Control)element).Template.FindName("PART_Header", (FrameworkElement)element);
            }

            return element;
        }

        private static bool IsTopHalf(UIElement element, DragEventArgs e)
        {

            double localPositionY = e.GetPosition(element).Y;
            return localPositionY < (element.RenderSize.Height / 2.0f);
        }

        private static UIElement GetItemFromY(ItemsControl itemsControl, double y)
        {
            UIElement hitResult = null;
            VisualTreeHelper.HitTest(itemsControl,
                null,
                result =>
                    {
                        var element = result.VisualHit as UIElement;

                        while (element != null)
                        {
                            if (element is TreeViewItem || element is ListBoxItem)
                            {
                                hitResult = element;
                                break;
                            }

                            element = VisualTreeHelper.GetParent(element) as UIElement;
                        }
                        if (hitResult != null && !hitResult.IsVisible)
                            hitResult = null;
                        return hitResult == null ? HitTestResultBehavior.Continue : HitTestResultBehavior.Stop;
                    },
                new GeometryHitTestParameters(new LineGeometry(new Point(0.0f, y), new Point(itemsControl.RenderSize.Width, y))));
            
            return hitResult;
        }
    }
}